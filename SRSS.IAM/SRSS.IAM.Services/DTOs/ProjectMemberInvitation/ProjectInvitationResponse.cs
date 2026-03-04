using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Services.DTOs.ProjectMemberInvitation
{
    public class ProjectInvitationResponse
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string ProjectTitle { get; set; } = null!;
        public Guid InvitedUserId { get; set; }
        public string InvitedUserFullName { get; set; } = null!;
        public string InvitedUserEmail { get; set; } = null!;
        public Guid InvitedByUserId { get; set; }
        public string InvitedByUserFullName { get; set; } = null!;
        public ProjectMemberInvitationStatus Status { get; set; }
        public ProjectRole Role { get; set; }
        public string? ResponseMessage { get; set; }
        public DateTimeOffset? ExpiredAt { get; set; }
        public DateTimeOffset? RespondedAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
