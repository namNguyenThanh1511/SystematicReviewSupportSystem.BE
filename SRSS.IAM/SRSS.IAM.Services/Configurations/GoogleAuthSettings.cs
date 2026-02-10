namespace SRSS.IAM.Services.Configurations
{
    public class GoogleAuthSettings
    {
        public const string SectionName = "GoogleAuthSettings";
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string AuthorizationEndpoint { get; set; } = "https://accounts.google.com/o/oauth2/v2/auth";
        public string Scope { get; set; } = "openid profile email";
    }
}
