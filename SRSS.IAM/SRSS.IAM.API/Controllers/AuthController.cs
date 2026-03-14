using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
using SRSS.IAM.Services.AuthService;
using SRSS.IAM.Services.DTOs.Auth;
using SRSS.IAM.Services.DTOs.User;
using SRSS.IAM.Services.JWTService;
using SRSS.IAM.Services.RefreshTokenService;
using System.Security.Claims;

namespace SRSS.IAM.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly IJwtService _jwtService;
        private readonly IRefreshTokenService _refreshTokenService;
        private static readonly string REFRESH_TOKEN_COOKIE_NAME = "SRSS_IAM_refreshToken";
        public AuthController(IAuthService authService, IJwtService jwtService, IRefreshTokenService refreshTokenService)
        {
            _authService = authService;
            _jwtService = jwtService;
            _refreshTokenService = refreshTokenService;
        }
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse>> Register([FromBody] RegisterRequest request)
        {
            await _authService.RegisterAsync(request);
            return Created("Đăng ký thành công");

        }
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            await IssueRefreshTokenAsync(result.UserId);
            return Ok(result, "Đăng nhập thành công");

        }

        ///<summary>
        /// Đăng nhập bằng Authentication Code của Google
        ///</summary>
        /// <param name="request">Authorization code sau khi người dùng hoàn thành OAuth flow và được Google gửi về</param>
        [HttpPost("google/login")]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            var result = await _authService.GoogleLoginAsync(request);
            await IssueRefreshTokenAsync(result.UserId);
            return Ok(result, "Đăng nhập bằng Google thành công");
        }

        /// <summary>
        /// Tạo Google OAuth URL để người dùng đăng nhập
        /// </summary>
        /// <param name="request">Request chứa redirectUrl - địa chỉ redirect về để nhận authorization code sau khi người dùng hoàn thành OAuth flow</param>
        [HttpPost("google/oauth-url")]
        public async Task<ActionResult<ApiResponse<GoogleOAuthUrlResponse>>> GenerateGoogleOAuthUrl([FromBody] GoogleOAuthUrlRequest request)
        {
            var result = await _authService.GenerateGoogleOAuthUrlAsync(request);
            return Ok(result, "Tạo Google OAuth URL thành công");
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> RefreshToken()
        {
            if (!Request.Cookies.TryGetValue(REFRESH_TOKEN_COOKIE_NAME, out var refreshToken))
            {
                throw new UnauthorizedAccessException("Refresh token is missing");
            }
            var validationResult = await _refreshTokenService.ValidateAsync(refreshToken);
            if (validationResult == null)
            {
                await ClearRefreshCookieAsync(refreshToken);
                throw new UnauthorizedAccessException("Invalid refresh token");
            }
            var result = await _authService.RefreshAsync(validationResult.UserId);
            await IssueRefreshTokenAsync(result.UserId);
            return Ok(result, "Làm mới token thành công");
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult<ApiResponse>> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Lấy access token từ header
            var accessToken = Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");
            if (string.IsNullOrEmpty(accessToken))
                return Unauthorized("Không tìm thấy access token");

            // Giải mã access token để lấy thời gian hết hạn
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(accessToken);
            var expUnix = jwtToken.Payload.Exp;
            if (expUnix == null)
                return BadRequest("Access token không hợp lệ");

            var expiry = DateTimeOffset.FromUnixTimeSeconds((long)expUnix);
            var ttl = expiry - DateTimeOffset.UtcNow;
            if (ttl <= TimeSpan.Zero)
                return BadRequest("Access token đã hết hạn");

            // Gọi service để thu hồi refresh token và blacklist access token
            await _authService.LogoutAsync(userId, accessToken, ttl);

            return Ok("Đăng xuất thành công");
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<UserProfileResponse>>> GetUserProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _authService.GetUserProfileAsync(userId);
            return Ok(result, "Lấy thông tin người dùng thành công");
        }

        private async Task IssueRefreshTokenAsync(Guid userId)
        {
            var issued = await _refreshTokenService.IssueAsync(userId);

            Response.Cookies.Append(
                REFRESH_TOKEN_COOKIE_NAME,
                issued.RefreshToken,
                BuildCookieOptions(issued.ExpiresAt)
            );
        }


        private static CookieOptions BuildCookieOptions(DateTimeOffset expiresUtc)
        {
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.None,
                Expires = expiresUtc
            };
        }

        private async Task ClearRefreshCookieAsync(string refreshToken)
        {
            await _refreshTokenService.RevokeAsync(refreshToken);
            Response.Cookies.Delete(REFRESH_TOKEN_COOKIE_NAME, BuildCookieOptions(DateTime.UtcNow.AddYears(-1)));
        }
    }
}
