using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Services.DTOs.Checklist;
using SRSS.IAM.Services.DTOs.Common;
using SRSS.IAM.Services.DTOs.Notification;
using SRSS.IAM.Services.DTOs.StudySelection;

namespace SRSS.IAM.Services.NotificationService
{
    public interface INotificationService
    {
        Task SendAsync(Guid userId, string title, string message, NotificationType type, Guid? relatedEntityId = null, NotificationEntityType? entityType = null, string? metadata = null);
        Task SendToManyAsync(IEnumerable<Guid> userIds, string title, string message, NotificationType type, Guid? relatedEntityId = null, NotificationEntityType? entityType = null, string? metadata = null);
        Task<PaginatedResponse<NotificationResponse>> GetMyNotificationsAsync(Guid userId, int pageNumber, int pageSize);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task MarkAsReadAsync(Guid notificationId, Guid userId);
        Task MarkAllAsReadAsync(Guid userId);

        Task RegisterConnectionAsync(Guid userId, string connectionId, CancellationToken cancellationToken = default);
        Task UnregisterConnectionAsync(string connectionId, CancellationToken cancellationToken = default);
        Task AddConnectionToGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default);
        Task RemoveConnectionFromGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default);
        Task SendMessageToAllAsync(string message, CancellationToken cancellationToken = default);
        Task SendMessageToUserAsync(Guid userId, string message, CancellationToken cancellationToken = default);
        Task SendMessageToGroupAsync(string groupName, string message, CancellationToken cancellationToken = default);
        Task SendMetadataExtractedAsync(Guid userId, ExtractionSuggestionResponse suggestion);
        Task SendChecklistAutoFillStatusAsync(Guid userId, ChecklistAutoFillStatusDto status);
    }
}
