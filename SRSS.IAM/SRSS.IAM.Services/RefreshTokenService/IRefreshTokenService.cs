using SRSS.IAM.Services.DTOs.Auth;

namespace SRSS.IAM.Services.RefreshTokenService
{
    public interface IRefreshTokenService
    {
        Task<RefreshTokenIssueResult> IssueAsync(Guid userId);
        Task<RefreshTokenValidationResult?> ValidateAsync(string refreshToken);
        Task RevokeAsync(string refreshToken);
        Task<bool> IsRevokeAsync(string refreshToken);
    }
}
