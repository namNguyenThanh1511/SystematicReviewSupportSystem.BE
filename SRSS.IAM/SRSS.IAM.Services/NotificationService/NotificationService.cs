using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.NotificationRepo;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.Checklist;
using SRSS.IAM.Services.DTOs.Common;
using SRSS.IAM.Services.DTOs.Notification;
using SRSS.IAM.Services.DTOs.StudySelection;

namespace SRSS.IAM.Services.NotificationService
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IUserConnectionRepository _userConnectionRepository;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IUnitOfWork unitOfWork,
            IHubContext<NotificationHub> hubContext,
            IUserConnectionRepository userConnectionRepository,
            ILogger<NotificationService> logger)
        {
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
            _userConnectionRepository = userConnectionRepository;
            _logger = logger;
        }

        public async Task SendAsync(Guid userId, string title, string message, NotificationType type, Guid? relatedEntityId = null, NotificationEntityType? entityType = null, string? metadata = null)
        {
            var notification = new Notification(userId, title, message, type, relatedEntityId, entityType, metadata);
            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            await SendMessageToUserAsync(userId, message);
        }

        public async Task SendToManyAsync(IEnumerable<Guid> userIds, string title, string message, NotificationType type, Guid? relatedEntityId = null, NotificationEntityType? entityType = null, string? metadata = null)
        {
            var userIdList = userIds.Distinct().ToList();
            var notifications = userIdList.Select(userId => new Notification(userId, title, message, type, relatedEntityId, entityType, metadata)).ToList();

            await _unitOfWork.Notifications.AddRangeAsync(notifications);
            await _unitOfWork.SaveChangesAsync();

            foreach (var userId in userIdList)
            {
                await SendMessageToUserAsync(userId, message);
            }
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

        public async Task RegisterConnectionAsync(Guid userId, string connectionId, CancellationToken cancellationToken = default)
        {
            await _userConnectionRepository.AddConnectionAsync(userId, connectionId, cancellationToken);
        }

        public async Task UnregisterConnectionAsync(string connectionId, CancellationToken cancellationToken = default)
        {
            await _userConnectionRepository.RemoveConnectionAsync(connectionId, cancellationToken);
        }

        public async Task AddConnectionToGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default)
        {
            await _hubContext.Groups.AddToGroupAsync(connectionId, groupName, cancellationToken);
            _logger.LogInformation("Connection {ConnectionId} joined group {GroupName}.", connectionId, groupName);
        }

        public async Task RemoveConnectionFromGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default)
        {
            await _hubContext.Groups.RemoveFromGroupAsync(connectionId, groupName, cancellationToken);
            _logger.LogInformation("Connection {ConnectionId} left group {GroupName}.", connectionId, groupName);
        }

        public async Task SendMessageToAllAsync(string message, CancellationToken cancellationToken = default)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", message, cancellationToken);
            _logger.LogInformation("SignalR message sent to all clients.");
        }

        public async Task SendMessageToUserAsync(Guid userId, string message, CancellationToken cancellationToken = default)
        {
            var connections = await _userConnectionRepository.GetConnectionsAsync(userId, cancellationToken);
            if (connections.Count == 0)
            {
                _logger.LogWarning("No active SignalR connections found for user {UserId}.", userId);
                return;
            }

            await _hubContext.Clients.Clients(connections).SendAsync("ReceiveMessage", message, cancellationToken);
            _logger.LogInformation("SignalR message sent to user {UserId} on {ConnectionCount} connection(s).", userId, connections.Count);
        }

        public async Task SendMessageToGroupAsync(string groupName, string message, CancellationToken cancellationToken = default)
        {
            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveMessage", message, cancellationToken);
            _logger.LogInformation("SignalR message sent to group {GroupName}.", groupName);
        }

        public async Task SendMetadataExtractedAsync(Guid userId, ExtractionSuggestionResponse suggestion)
        {
            var connections = await _userConnectionRepository.GetConnectionsAsync(userId);
            if (connections.Count == 0)
            {
                _logger.LogWarning("No active SignalR connections found for user {UserId} to send metadata update.", userId);
                return;
            }

            await _hubContext.Clients.Clients(connections).SendAsync("OnMetadataExtracted", new
            {
                paperId = suggestion.PaperId,
                suggestion
            });
            _logger.LogInformation("SignalR metadata update sent to user {UserId} on {ConnectionCount} connection(s).", userId, connections.Count);
        }

        public async Task SendChecklistAutoFillStatusAsync(Guid userId, ChecklistAutoFillStatusDto status)
        {
            var connections = await _userConnectionRepository.GetConnectionsAsync(userId);
            if (connections.Count == 0)
            {
                _logger.LogWarning("No active SignalR connections found for user {UserId} to send checklist auto-fill status.", userId);
                return;
            }

            await _hubContext.Clients.Clients(connections).SendAsync("OnChecklistAutoFillStatus", status);
            _logger.LogInformation(
                "SignalR checklist auto-fill status '{Status}' sent to user {UserId} for checklist {ChecklistId}.",
                status.Status, userId, status.ReviewChecklistId);
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
                    RelatedEntityId = entity.RelatedEntityId,
                    EntityType = entity.EntityType,
                    IsRead = entity.IsRead,
                    CreatedAt = entity.CreatedAt
                };
            }
        }
    }
}
