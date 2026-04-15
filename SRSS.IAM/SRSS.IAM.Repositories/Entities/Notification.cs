using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
    public class Notification : BaseEntity<Guid>
    {
        public Guid UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public NotificationType Type { get; set; }

        public Guid? RelatedEntityId { get; private set; }
        public NotificationEntityType? EntityType { get; private set; }

        public bool IsRead { get; set; }
        public DateTimeOffset? ReadAt { get; set; }
        public string? Metadata { get; set; }

        public User User { get; set; }

        private Notification()
        {
            Title = null!;
            Message = null!;
            User = null!;
        }

        public Notification(
            Guid userId,
            string title,
            string message,
            NotificationType type,
            Guid? relatedEntityId = null,
            NotificationEntityType? entityType = null,
            string? metadata = null)
        {
            UserId = userId;
            Title = title;
            Message = message;
            Type = type;
            RelatedEntityId = relatedEntityId;
            EntityType = entityType;
            Metadata = metadata;
            IsRead = false;
            User = null!;
        }
    }

    public enum NotificationType
    {
        System = 1,
        Project = 2,
        Invitation = 3,
        Review = 4,
        Comment = 5
    }

    public enum NotificationEntityType
    {
        ProjectInvitation = 1,
        Project = 2,
        PaperAssignment = 3,
        Paper = 4
    }
}
