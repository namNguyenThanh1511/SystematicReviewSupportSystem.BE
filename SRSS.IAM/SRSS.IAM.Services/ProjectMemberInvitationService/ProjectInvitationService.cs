using Microsoft.EntityFrameworkCore;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.ProjectMemberInvitation;
using SRSS.IAM.Services.NotificationService;
using SRSS.IAM.Services.Mappers;
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
            // Existing implementation...
            var project = await _unitOfWork.SystematicReviewProjects.FindSingleAsync(p => p.Id == projectId, isTracking: true);
            if (project == null)
            {
                throw new InvalidOperationException($"Project with ID {projectId} not found.");
            }

            var inviter = await _unitOfWork.Users.FindSingleAsync(u => u.Id == inviterUserId);
            if (inviter == null)
            {
                throw new InvalidOperationException("Inviter not found.");
            }

            var inviterProjectMembers = await _unitOfWork.SystematicReviewProjects.GetMembersByProjectIdAsync(projectId);
            var inviterMember = inviterProjectMembers.FirstOrDefault(m => m.UserId == inviterUserId);

            bool isSystemAdmin = inviter.Role == Role.Admin;
            bool isProjectLeader = inviterMember?.Role == ProjectRole.Leader;

            if (!isSystemAdmin && !isProjectLeader)
            {
                throw new InvalidOperationException("Unauthorized: Only Admins or Project Leaders can invite members.");
            }

            if (!isSystemAdmin && request.Role == ProjectRole.Leader)
            {
                throw new InvalidOperationException("Unauthorized: Only Admins can invite a Project Leader.");
            }

            var invitations = new List<ProjectMemberInvitation>();
            var members = await _unitOfWork.SystematicReviewProjects.GetMembersByProjectIdAsync(projectId);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                foreach (var userId in request.UserIds)
                {
                    if (userId == inviterUserId)
                    {
                        throw new ArgumentException("Inviter cannot invite themselves.");
                    }

                    var invitedUser = await _unitOfWork.Users.FindSingleAsync(u => u.Id == userId);
                    if (invitedUser == null)
                    {
                        throw new ArgumentException($"User with ID {userId} not found.");
                    }

                    if (members.Any(m => m.UserId == userId))
                    {
                        throw new ArgumentException($"User with ID {userId} is already a member of this project.");
                    }

                    if (await _unitOfWork.SystematicReviewProjects.ExistsPendingInvitationAsync(projectId, userId))
                    {
                        throw new ArgumentException($"User with ID {userId} already has a pending invitation for this project.");
                    }

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

                try
                {
                    foreach (var invitation in invitations)
                    {
                        var title = "Project Invitation";
                        var message = $"You have been invited to join project {project.Title} as {invitation.Role}";
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
                            invitation.Id,
                            NotificationEntityType.ProjectInvitation,
                            JsonSerializer.Serialize(metadataObj));
                    }
                }
                catch (Exception) { /* Fail-safe */ }
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<IEnumerable<ProjectInvitationResponse>> GetProjectInvitationsAsync(Guid projectId, Guid currentUserId, ProjectMemberInvitationStatus? status = null)
        {
            var user = await _unitOfWork.Users.FindSingleAsync(u => u.Id == currentUserId);
            var members = await _unitOfWork.SystematicReviewProjects.GetMembersByProjectIdAsync(projectId);
            var member = members.FirstOrDefault(m => m.UserId == currentUserId);

            if (user?.Role != Role.Admin && member?.Role != ProjectRole.Leader)
            {
                throw new InvalidOperationException("Unauthorized: Only Admins or Project Leaders can view project invitations.");
            }

            var invitations = await _unitOfWork.ProjectMemberInvitations.GetByProjectIdAsync(projectId, status);
            return invitations.ToResponseList();
        }

        public async Task<ProjectInvitationResponse> GetByIdAsync(Guid invitationId, Guid currentUserId)
        {
            var invitation = await _unitOfWork.ProjectMemberInvitations.GetByIdWithDetailsAsync(invitationId);

            if (invitation == null)
            {
                throw new InvalidOperationException($"Invitation with ID {invitationId} not found.");
            }

            var user = await _unitOfWork.Users.FindSingleAsync(u => u.Id == currentUserId);

            if (user?.Role != Role.Admin && invitation.InvitedUserId != currentUserId)
            {
                throw new InvalidOperationException("Unauthorized: You do not have permission to view this invitation.");
            }

            return invitation.ToResponse();
        }

        public async Task AcceptInvitationAsync(Guid invitationId, Guid currentUserId)
        {
            var invitation = await _unitOfWork.ProjectMemberInvitations.GetByIdWithDetailsAsync(invitationId);

            if (invitation == null)
                throw new InvalidOperationException("Invitation not found.");

            if (invitation.InvitedUserId != currentUserId)
                throw new InvalidOperationException("Unauthorized: This invitation is not for you.");

            if (invitation.Status != ProjectMemberInvitationStatus.Pending)
                throw new InvalidOperationException($"Cannot accept invitation in {invitation.Status} status.");

            if (invitation.ExpiredAt.HasValue && invitation.ExpiredAt < DateTimeOffset.UtcNow)
            {
                invitation.Expire();
                await _unitOfWork.SaveChangesAsync();
                throw new InvalidOperationException("This invitation has expired.");
            }

            var members = await _unitOfWork.SystematicReviewProjects.GetMembersByProjectIdAsync(invitation.ProjectId);
            if (members.Any(m => m.UserId == currentUserId))
            {
                throw new InvalidOperationException("You are already a member of this project.");
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                invitation.Accept();
                var newMember = new ProjectMember(invitation.ProjectId, invitation.InvitedUserId, invitation.Role);

                await _unitOfWork.SystematicReviewProjects.AddMemberAsync(newMember);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                // Notify inviter
                try
                {
                    var metadataObj = new
                    {
                        projectId = invitation.ProjectId,
                        invitationId = invitation.Id,
                        role = invitation.Role.ToString()
                    };

                    await _notificationService.SendAsync(
                        invitation.InvitedByUserId,
                        "Invitation Accepted",
                        $"{invitation.InvitedUser.FullName} has accepted your invitation to join {invitation.Project.Title}.",
                        NotificationType.Invitation,
                        invitation.Id,
                        NotificationEntityType.ProjectInvitation,
                        JsonSerializer.Serialize(metadataObj));
                }
                catch { }
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task RejectInvitationAsync(Guid invitationId, Guid currentUserId, RejectInvitationRequest request)
        {
            var invitation = await _unitOfWork.ProjectMemberInvitations.GetByIdWithDetailsAsync(invitationId);

            if (invitation == null)
                throw new InvalidOperationException("Invitation not found.");

            if (invitation.InvitedUserId != currentUserId)
                throw new InvalidOperationException("Unauthorized.");

            if (invitation.Status != ProjectMemberInvitationStatus.Pending)
                throw new InvalidOperationException("Invitation is no longer pending.");

            invitation.Reject(request.ResponseMessage);
            await _unitOfWork.SaveChangesAsync();

            // Notify inviter
            try
            {
                var metadataObj = new
                {
                    projectId = invitation.ProjectId,
                    invitationId = invitation.Id,
                    role = invitation.Role.ToString()
                };

                await _notificationService.SendAsync(
                    invitation.InvitedByUserId,
                    "Invitation Rejected",
                    $"{invitation.InvitedUser.FullName} has rejected your invitation to join {invitation.Project.Title}. Message: {invitation.ResponseMessage ?? "No message provided"}",
                    NotificationType.Invitation,
                    invitation.Id,
                    NotificationEntityType.ProjectInvitation,
                    JsonSerializer.Serialize(metadataObj));
            }
            catch { }
        }

        public async Task CancelInvitationAsync(Guid invitationId, Guid currentUserId)
        {
            var invitation = await _unitOfWork.ProjectMemberInvitations.GetByIdWithDetailsAsync(invitationId);

            if (invitation == null)
                throw new InvalidOperationException("Invitation not found.");

            var user = await _unitOfWork.Users.FindSingleAsync(u => u.Id == currentUserId);
            var members = await _unitOfWork.SystematicReviewProjects.GetMembersByProjectIdAsync(invitation.ProjectId);
            var member = members.FirstOrDefault(m => m.UserId == currentUserId);

            if (user?.Role != Role.Admin && member?.Role != ProjectRole.Leader)
            {
                throw new InvalidOperationException("Unauthorized: Only Admins or Project Leaders can cancel invitations.");
            }

            if (invitation.Status != ProjectMemberInvitationStatus.Pending)
                throw new InvalidOperationException("Only Pending invitations can be cancelled.");

            invitation.Cancel();
            await _unitOfWork.SaveChangesAsync();

            // Notify invited user
            try
            {
                var metadataObj = new
                {
                    projectId = invitation.ProjectId,
                    invitationId = invitation.Id,
                    role = invitation.Role.ToString()
                };

                await _notificationService.SendAsync(
                    invitation.InvitedUserId,
                    "Invitation Cancelled",
                    $"The invitation to join project {invitation.Project.Title} has been cancelled.",
                    NotificationType.Invitation,
                    invitation.Id,
                    NotificationEntityType.ProjectInvitation,
                    JsonSerializer.Serialize(metadataObj));
            }
            catch { }
        }
    }
}
