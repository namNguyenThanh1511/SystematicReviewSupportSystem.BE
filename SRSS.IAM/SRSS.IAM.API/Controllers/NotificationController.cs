using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
using SRSS.IAM.Services.DTOs.Common;
using SRSS.IAM.Services.DTOs.Notification;
using SRSS.IAM.Services.NotificationService;
using SRSS.IAM.Services.UserService;

namespace SRSS.IAM.API.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    public class NotificationController : BaseController
    {
        private readonly INotificationService _notificationService;
        private readonly ICurrentUserService _currentUserService;

        public NotificationController(INotificationService notificationService, ICurrentUserService currentUserService)
        {
            _notificationService = notificationService;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<NotificationResponse>>>> GetMyNotifications([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var (userId, _) = _currentUserService.GetCurrentUser();
            var result = await _notificationService.GetMyNotificationsAsync(Guid.Parse(userId), pageNumber, pageSize);
            return Ok(result, "Notifications retrieved successfully.");
        }

        [HttpGet("unread-count")]
        public async Task<ActionResult<ApiResponse<int>>> GetUnreadCount()
        {
            var (userId, _) = _currentUserService.GetCurrentUser();
            var count = await _notificationService.GetUnreadCountAsync(Guid.Parse(userId));
            return Ok(count, "Unread count retrieved successfully.");
        }

        [HttpPut("{id}/read")]
        public async Task<ActionResult<ApiResponse>> MarkAsRead(Guid id)
        {
            var (userId, _) = _currentUserService.GetCurrentUser();
            await _notificationService.MarkAsReadAsync(id, Guid.Parse(userId));
            return Ok("Notification marked as read successfully.");
        }

        [HttpPut("read-all")]
        public async Task<ActionResult<ApiResponse>> MarkAllAsRead()
        {
            var (userId, _) = _currentUserService.GetCurrentUser();
            await _notificationService.MarkAllAsReadAsync(Guid.Parse(userId));
            return Ok("All notifications marked as read successfully.");
        }

        [HttpPost("send-all")]
        public async Task<ActionResult<ApiResponse>> SendAll([FromBody] SendAllNotificationRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                throw new ArgumentException("Message is required.");
            }

            await _notificationService.SendMessageToAllAsync(request.Message, cancellationToken);
            return Ok("Message sent to all users successfully.");
        }

        [HttpPost("send-user")]
        public async Task<ActionResult<ApiResponse>> SendUser([FromBody] SendUserNotificationRequest request, CancellationToken cancellationToken)
        {
            if (request.UserId == Guid.Empty)
            {
                throw new ArgumentException("UserId is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Message))
            {
                throw new ArgumentException("Message is required.");
            }

            await _notificationService.SendMessageToUserAsync(request.UserId, request.Message, cancellationToken);
            return Ok("Message sent to user successfully.");
        }

        [HttpPost("send-group")]
        public async Task<ActionResult<ApiResponse>> SendGroup([FromBody] SendGroupNotificationRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.GroupName))
            {
                throw new ArgumentException("GroupName is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Message))
            {
                throw new ArgumentException("Message is required.");
            }

            await _notificationService.SendMessageToGroupAsync(request.GroupName, request.Message, cancellationToken);
            return Ok("Message sent to group successfully.");
        }
    }
}
