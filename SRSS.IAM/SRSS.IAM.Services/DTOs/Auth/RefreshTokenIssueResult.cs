namespace SRSS.IAM.Services.DTOs.Auth
{
    public class RefreshTokenIssueResult
    {
        public string RefreshToken { get; init; } = string.Empty;
        public DateTimeOffset ExpiresAt { get; init; }
    }

    public class RefreshTokenValidationResult
    {
        public Guid UserId { get; init; }
        public DateTimeOffset ExpiresAt { get; init; }
    }
}
