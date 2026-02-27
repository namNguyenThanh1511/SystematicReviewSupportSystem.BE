using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
    public class ProjectMemberInvitation : BaseEntity<Guid>
    {
        public Guid ProjectId { get; private set; }
        public Guid InvitedUserId { get; private set; }
        public Guid InvitedByUserId { get; private set; }
        public ProjectMemberInvitationStatus Status { get; private set; }
        public ProjectRole Role { get; private set; }
        public string? ResponseMessage { get; private set; }
        public DateTimeOffset? ExpiredAt { get; private set; }
        public DateTimeOffset? RespondedAt { get; private set; }

        // Navigation properties
        public SystematicReviewProject Project { get; private set; } = null!;
        public User InvitedUser { get; private set; } = null!;
        public User InvitedByUser { get; private set; } = null!;

        private ProjectMemberInvitation() { }

        public ProjectMemberInvitation(
            Guid projectId,
            Guid invitedUserId,
            Guid invitedByUserId,
            ProjectRole role,
            DateTimeOffset? expiredAt)
        {
            Id = Guid.NewGuid();
            ProjectId = projectId;
            InvitedUserId = invitedUserId;
            InvitedByUserId = invitedByUserId;
            Role = role;
            ExpiredAt = expiredAt;
            Status = ProjectMemberInvitationStatus.Pending;
            CreatedAt = DateTimeOffset.UtcNow;
        }

        public void Accept()
        {
            Status = ProjectMemberInvitationStatus.Accepted;
            RespondedAt = DateTimeOffset.UtcNow;
        }

        public void Reject(string? responseMessage)
        {
            Status = ProjectMemberInvitationStatus.Rejected;
            ResponseMessage = responseMessage;
            RespondedAt = DateTimeOffset.UtcNow;
        }

        public void Cancel()
        {
            Status = ProjectMemberInvitationStatus.Cancelled;
            RespondedAt = DateTimeOffset.UtcNow;
        }

        public void Expire()
        {
            Status = ProjectMemberInvitationStatus.Expired;
        }
    }

    public enum ProjectMemberInvitationStatus
    {
        Pending = 1,
        Accepted = 2,
        Rejected = 3,
        Cancelled = 4,
        Expired = 5
    }
}
