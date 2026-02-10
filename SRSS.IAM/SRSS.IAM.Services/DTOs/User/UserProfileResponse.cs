namespace SRSS.IAM.Services.DTOs.User
{
    public class UserProfileResponse
    {
        public Guid Id { get; set; }
        public string? Email { get; set; } = string.Empty;
        public string? Username { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
