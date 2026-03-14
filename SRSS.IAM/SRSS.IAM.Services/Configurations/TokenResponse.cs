namespace SRSS.IAM.Services.Configurations
{
    public class TokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTimeOffset AccessTokenExpiry { get; set; }
        public DateTimeOffset RefreshTokenExpiry { get; set; }
        public string TokenType { get; set; } = "Bearer";
    }
}
