using Microsoft.EntityFrameworkCore;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.ProjectMemberInvitation;
using SRSS.IAM.Services.NotificationService;
using System.Text.Json;

namespace SRSS.IAM.Services.ProjectMemberInvitationService
{
    public class ProjectInvitationService : IProjectInvitationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;

        public ProjectInvitationService(IUnitOfWork unitOfWork, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }

        public async Task CreateInvitationsAsync(Guid projectId, Guid inviterUserId, CreateProjectInvitationRequest request)
        {
            // 1. Load project
            var project = await _unitOfWork.SystematicReviewProjects.FindSingleAsync(p => p.Id == projectId, isTracking: true);
            if (project == null)
            {
                throw new InvalidOperationException($"Project with ID {projectId} not found.");
            }

            // 2. Determine inviter role
            var inviter = await _unitOfWork.Users.FindSingleAsync(u => u.Id == inviterUserId);
            if (inviter == null)
            {
                throw new InvalidOperationException("Inviter not found.");
            }

            var inviterProjectMember = await _unitOfWork.SystematicReviewProjects.GetMembersByProjectIdAsync(projectId);
            var inviterMember = inviterProjectMember.FirstOrDefault(m => m.UserId == inviterUserId);

            bool isSystemAdmin = inviter.Role == Role.Admin;
            bool isProjectLeader = inviterMember?.Role == ProjectRole.Leader;

            // 3. Permission check
            if (!isSystemAdmin && !isProjectLeader)
            {
                throw new InvalidOperationException("Unauthorized: Only Admins or Project Leaders can invite members.");
            }

            if (!isSystemAdmin && request.Role == ProjectRole.Leader)
            {
                throw new InvalidOperationException("Unauthorized: Only Admins can invite a Project Leader.");
            }

            // 4. Validate each user and create invitations
            var invitations = new List<ProjectMemberInvitation>();
            var members = await _unitOfWork.SystematicReviewProjects.GetMembersByProjectIdAsync(projectId);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                foreach (var userId in request.UserIds)
                {
                    // Basic validations
                    if (userId == inviterUserId)
                    {
                        throw new ArgumentException("Inviter cannot invite themselves.");
                    }

                    var invitedUser = await _unitOfWork.Users.FindSingleAsync(u => u.Id == userId);
                    if (invitedUser == null)
                    {
                        throw new ArgumentException($"User with ID {userId} not found.");
                    }

                    // User must not already be project member
                    if (members.Any(m => m.UserId == userId))
                    {
                        throw new ArgumentException($"User with ID {userId} is already a member of this project.");
                    }

                    // No existing Pending invitation
                    if (await _unitOfWork.SystematicReviewProjects.ExistsPendingInvitationAsync(projectId, userId))
                    {
                        throw new ArgumentException($"User with ID {userId} already has a pending invitation for this project.");
                    }

                    // Single leader per project rule
                    if (request.Role == ProjectRole.Leader)
                    {
                        if (await _unitOfWork.SystematicReviewProjects.ProjectHasLeaderAsync(projectId))
                        {
                            throw new ArgumentException("Project already has a leader.");
                        }

                        if (await _unitOfWork.SystematicReviewProjects.HasPendingLeaderInvitationAsync(projectId))
                        {
                            throw new ArgumentException("Project already has a pending leader invitation.");
                        }
                    }

                    var invitation = new ProjectMemberInvitation(
                        projectId,
                        userId,
                        inviterUserId,
                        request.Role,
                        request.ExpiredAt);

                    invitations.Add(invitation);
                }

                await _unitOfWork.ProjectMemberInvitations.AddRangeAsync(invitations);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                // Send notifications
                try
                {
                    foreach (var invitation in invitations)
                    {
                        var title = "Project Invitation";
                        var message = $"You have been invited to join project {project.Title} as {invitation.Role}";
                        var navigationUrl = $"/project/{projectId}/my-invitation";
                        var metadataObj = new
                        {
                            projectId = projectId,
                            invitationId = invitation.Id,
                            role = invitation.Role.ToString()
                        };

                        await _notificationService.SendAsync(
                            invitation.InvitedUserId,
                            title,
                            message,
                            NotificationType.Invitation,
                            navigationUrl,
                            JsonSerializer.Serialize(metadataObj));
                    }
                }
                catch (Exception)
                {
                    // Fail safe: notification failure should not break the invitation flow
                    // In real production, we might want to log this using a logging framework
                }
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
