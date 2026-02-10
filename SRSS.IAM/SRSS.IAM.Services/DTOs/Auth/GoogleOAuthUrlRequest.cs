using System.ComponentModel.DataAnnotations;

namespace SRSS.IAM.Services.DTOs.Auth
{
    public class GoogleOAuthUrlRequest
    {
        [Required(ErrorMessage = "RedirectUrl là bắt buộc")]
        [Url(ErrorMessage = "RedirectUrl phải là một URL hợp lệ")]
        public string RedirectUrl { get; set; } // Địa chỉ redirect về để nhận IdToken sau khi người dùng hoàn thành Oauth flow
    }
}
