namespace SRSS.IAM.Services.DTOs.Notification
{
    public class SendGroupNotificationRequest
    {
        public string GroupName { get; set; } = null!;
        public string Message { get; set; } = null!;
    }
}
