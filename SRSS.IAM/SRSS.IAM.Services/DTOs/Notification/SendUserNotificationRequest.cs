namespace SRSS.IAM.Services.DTOs.Notification
{
    public class SendUserNotificationRequest
    {
        public Guid UserId { get; set; }
        public string Message { get; set; } = null!;
    }
}
