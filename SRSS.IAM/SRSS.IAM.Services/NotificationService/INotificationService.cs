using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Services.DTOs.Common;
using SRSS.IAM.Services.DTOs.Notification;

namespace SRSS.IAM.Services.NotificationService
{
    public interface INotificationService
    {
        Task SendAsync(Guid userId, string title, string message, NotificationType type, string? navigationUrl = null, string? metadata = null);
        Task SendToManyAsync(IEnumerable<Guid> userIds, string title, string message, NotificationType type, string? navigationUrl = null, string? metadata = null);
        Task<PaginatedResponse<NotificationResponse>> GetMyNotificationsAsync(Guid userId, int pageNumber, int pageSize);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task MarkAsReadAsync(Guid notificationId, Guid userId);
        Task MarkAllAsReadAsync(Guid userId);
    }
}
