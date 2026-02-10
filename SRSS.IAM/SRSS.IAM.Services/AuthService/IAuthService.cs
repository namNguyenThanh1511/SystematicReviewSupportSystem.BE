using SRSS.IAM.Services.DTOs.Auth;
using SRSS.IAM.Services.DTOs.User;

namespace SRSS.IAM.Services.AuthService
{
    public interface IAuthService
    {
        Task RegisterAsync(RegisterRequest request);
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<LoginResponse> GoogleLoginAsync(GoogleLoginRequest request);
        Task<GoogleOAuthUrlResponse> GenerateGoogleOAuthUrlAsync(GoogleOAuthUrlRequest request);
        Task<LoginResponse> RefreshAsync(Guid userId);
        Task LogoutAsync(string userId, string accessToken, TimeSpan accessTokenTtl);
        Task<UserProfileResponse> GetUserProfileAsync(string userId);
    }
}
