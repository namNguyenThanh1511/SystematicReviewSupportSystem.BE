namespace SRSS.IAM.Services.DTOs.Auth
{
    // DTO response cho đăng nhập thành công (cũng dùng cho verify registration)
    public class LoginResponse
    {
        public Guid UserId { get; set; } // ID tài khoản
        public string Username { get; set; } = null!; // Tên người dùng
        public string Email { get; set; } = null!; // Email
        public string AccessToken { get; set; } = null!; // JWT token
        public DateTimeOffset AccessTokenExpiresAt { get; set; }
        public string Role { get; set; }

    }
}
