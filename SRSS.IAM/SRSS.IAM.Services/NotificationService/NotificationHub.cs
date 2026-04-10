using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace SRSS.IAM.Services.NotificationService
{
    public class NotificationHub : Hub
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(
            INotificationService notificationService,
            ILogger<NotificationHub> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userIdValue = Context.UserIdentifier
                    ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? Context.GetHttpContext()?.Request.Query["userId"].FirstOrDefault();
            _logger.LogInformation("User connected: {UserId}, ConnectionId: {ConnectionId}", Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, Context.ConnectionId);
            if (Context.User?.Identity?.IsAuthenticated != true)
            {
                _logger.LogWarning("User NOT authenticated in SignalR!");
            }
            if (Guid.TryParse(userIdValue, out var userId))
            {
                await _notificationService.RegisterConnectionAsync(userId, Context.ConnectionId);
                _logger.LogInformation("SignalR connection {ConnectionId} registered for user {UserId}.", Context.ConnectionId, userId);
            }
            else
            {
                _logger.LogWarning("SignalR connection {ConnectionId} established without valid user mapping.", Context.ConnectionId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await _notificationService.UnregisterConnectionAsync(Context.ConnectionId);
            _logger.LogInformation("SignalR connection {ConnectionId} removed.", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinGroup(string groupName)
        {
            if (string.IsNullOrWhiteSpace(groupName))
            {
                throw new ArgumentException("Group name is required.", nameof(groupName));
            }

            await _notificationService.AddConnectionToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task LeaveGroup(string groupName)
        {
            if (string.IsNullOrWhiteSpace(groupName))
            {
                throw new ArgumentException("Group name is required.", nameof(groupName));
            }

            await _notificationService.RemoveConnectionFromGroupAsync(Context.ConnectionId, groupName);
        }
    }
}
