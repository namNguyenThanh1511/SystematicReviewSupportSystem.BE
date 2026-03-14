using System.ComponentModel.DataAnnotations;

namespace SRSS.IAM.Services.DTOs.Auth
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Email/Username không được để trống.")]
        [StringLength(255, ErrorMessage = "Email/username quá dài.")]
        [RegularExpression(@"^\S+$", ErrorMessage = "Email/Username không được có space.")]
        public string KeyLogin { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6-100 ký tự")]
        public string Password { get; set; } = string.Empty;
    }
}
