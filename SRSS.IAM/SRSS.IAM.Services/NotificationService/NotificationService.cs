using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.Common;
using SRSS.IAM.Services.DTOs.Notification;

namespace SRSS.IAM.Services.NotificationService
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public NotificationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task SendAsync(Guid userId, string title, string message, NotificationType type, string? navigationUrl = null, string? metadata = null)
        {
            var notification = new Notification(userId, title, message, type, navigationUrl, metadata);
            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task SendToManyAsync(IEnumerable<Guid> userIds, string title, string message, NotificationType type, string? navigationUrl = null, string? metadata = null)
        {
            var notifications = userIds.Select(userId => new Notification(userId, title, message, type, navigationUrl, metadata)).ToList();
            await _unitOfWork.Notifications.AddRangeAsync(notifications);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<PaginatedResponse<NotificationResponse>> GetMyNotificationsAsync(Guid userId, int pageNumber, int pageSize)
        {
            var notifications = await _unitOfWork.Notifications.GetByUserIdAsync(userId, pageNumber, pageSize);
            var totalCount = await _unitOfWork.Notifications.CountByUserIdAsync(userId);

            // Mapping
            var items = notifications.Select(Mapper.ToResponse).ToList();

            return new PaginatedResponse<NotificationResponse>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _unitOfWork.Notifications.GetUnreadCountAsync(userId);
        }

        public async Task MarkAsReadAsync(Guid notificationId, Guid userId)
        {
            var notification = await _unitOfWork.Notifications.FindSingleAsync(n => n.Id == notificationId);

            if (notification == null)
            {
                throw new InvalidOperationException($"Notification with ID {notificationId} not found.");
            }

            if (notification.UserId != userId)
            {
                throw new UnauthorizedAccessException("You do not have permission to mark this notification as read.");
            }

            notification.IsRead = true;
            notification.ReadAt = DateTimeOffset.UtcNow;

            await _unitOfWork.Notifications.UpdateAsync(notification);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task MarkAllAsReadAsync(Guid userId)
        {
            await _unitOfWork.Notifications.MarkAllAsReadAsync(userId);
            await _unitOfWork.SaveChangesAsync();
        }

        // Internal Mapper class for simplicity or use existing Mapper pattern if found.
        private static class Mapper
        {
            public static NotificationResponse ToResponse(Notification entity)
            {
                return new NotificationResponse
                {
                    Id = entity.Id,
                    Title = entity.Title,
                    Message = entity.Message,
                    Type = entity.Type,
                    NavigationUrl = entity.NavigationUrl,
                    IsRead = entity.IsRead,
                    CreatedAt = entity.CreatedAt
                };
            }
        }
    }
}
