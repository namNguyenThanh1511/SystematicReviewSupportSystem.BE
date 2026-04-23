namespace SRSS.IAM.Services.OpenRouter;

public class OpenRouterOptions
{
    public const string SectionName = "OpenRouter";
    
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://openrouter.ai/api/v1";
    public string DefaultModel { get; set; } = "google/gemini-2.0-flash-001";
    public string SiteUrl { get; set; } = string.Empty;
    public string SiteName { get; set; } = string.Empty;
}
