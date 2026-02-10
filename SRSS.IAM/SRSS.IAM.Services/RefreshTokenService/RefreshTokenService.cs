using Microsoft.Extensions.Configuration;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.Auth;
using SRSS.IAM.Services.JWTService;
using System.Security.Cryptography;

namespace SRSS.IAM.Services.RefreshTokenService
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly TimeSpan _lifetime;
        private readonly IUnitOfWork _unitOfWork;
        private readonly long _refreshTokenLifetimeDays;
        private readonly IJwtService _jwtService;
        public RefreshTokenService(IConfiguration configuration, IUnitOfWork unitOfWork, IJwtService jwtService)
        {
            var refreshTokenExpirationDaysString = configuration["JwtSettings:RefreshTokenExpirationDays"] ?? "30";
            if (!long.TryParse(refreshTokenExpirationDaysString, out _refreshTokenLifetimeDays))
            {
                _refreshTokenLifetimeDays = 30;
            }
            _lifetime = TimeSpan.FromDays(_refreshTokenLifetimeDays);
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
        }
        // Issue a new refresh token based on the provided refresh token
        public async Task<RefreshTokenIssueResult> IssueAsync(Guid userId)
        {
            // ✅ 1. Sinh token ngẫu nhiên - cryptographically secure
            var refreshToken = GenerateRandomToken();

            // ✅ 2. Set thời gian chuẩn UTC
            var expiresAt = DateTimeOffset.UtcNow.AddDays(_refreshTokenLifetimeDays);

            // ✅ 3. Lấy user
            var user = await _unitOfWork.Users.FindSingleAsync(u => u.Id == userId)
                       ?? throw new UnauthorizedAccessException("User not found");

            // ✅ 4. Lưu refresh token vào DB
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = expiresAt;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // ✅ 5. Trả về đúng thứ cần dùng
            return new RefreshTokenIssueResult
            {
                RefreshToken = refreshToken,
                ExpiresAt = expiresAt
            };
        }

        public async Task RevokeAsync(string refreshToken)
        {
            var user = await _unitOfWork.Users.FindSingleAsync(u => u.RefreshToken == refreshToken)
                       ?? throw new UnauthorizedAccessException("User not found");
            user.IsRefreshTokenRevoked = true;
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<bool> IsRevokeAsync(string refreshToken)
        {
            User? user = await _unitOfWork.Users.FindSingleAsync(u => u.RefreshToken == refreshToken);
            if (user != null && user.IsRefreshTokenRevoked)
            {
                return true;
            }
            return false;

        }


        public async Task<RefreshTokenValidationResult?> ValidateAsync(string refreshToken)
        {
            var user = await _unitOfWork.Users.FindSingleAsync(u => u.RefreshToken == refreshToken);
            if (user == null) return null;

            if (user.IsRefreshTokenRevoked) return null;

            if (user.RefreshTokenExpiryTime == null || user.RefreshTokenExpiryTime < DateTimeOffset.UtcNow)
            {
                return null;
            }

            return new RefreshTokenValidationResult
            {
                UserId = user.Id,
                ExpiresAt = user.RefreshTokenExpiryTime.Value
            };
        }

        private static string GenerateRandomToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
}
