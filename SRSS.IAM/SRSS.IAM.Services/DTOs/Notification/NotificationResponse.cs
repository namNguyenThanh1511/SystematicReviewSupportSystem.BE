using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Services.DTOs.Notification
{
    public class NotificationResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public NotificationType Type { get; set; }
        public Guid? RelatedEntityId { get; set; }
        public NotificationEntityType? EntityType { get; set; }

        public bool IsRead { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
