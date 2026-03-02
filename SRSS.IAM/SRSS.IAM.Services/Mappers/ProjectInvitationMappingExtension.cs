using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Services.DTOs.ProjectMemberInvitation;

namespace SRSS.IAM.Services.Mappers
{
    public static class ProjectInvitationMappingExtension
    {
        public static ProjectInvitationResponse ToResponse(this ProjectMemberInvitation invitation)
        {
            return new ProjectInvitationResponse
            {
                Id = invitation.Id,
                ProjectId = invitation.ProjectId,
                ProjectTitle = invitation.Project?.Title ?? string.Empty,
                InvitedUserId = invitation.InvitedUserId,
                InvitedUserFullName = invitation.InvitedUser?.FullName ?? string.Empty,
                InvitedUserEmail = invitation.InvitedUser?.Email ?? string.Empty,
                InvitedByUserId = invitation.InvitedByUserId,
                InvitedByUserFullName = invitation.InvitedByUser?.FullName ?? string.Empty,
                Status = invitation.Status,
                Role = invitation.Role,
                ResponseMessage = invitation.ResponseMessage,
                ExpiredAt = invitation.ExpiredAt,
                RespondedAt = invitation.RespondedAt,
                CreatedAt = invitation.CreatedAt
            };
        }

        public static List<ProjectInvitationResponse> ToResponseList(this IEnumerable<ProjectMemberInvitation> invitations)
        {
            return invitations.Select(i => i.ToResponse()).ToList();
        }
    }
}
