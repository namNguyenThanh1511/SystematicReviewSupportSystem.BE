namespace SRSS.IAM.Services.OpenRouter;

public class OpenRouterOptions
{
    public const string SectionName = "OpenRouter";
    
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://openrouter.ai/api/v1";
    public string DefaultModel { get; set; } = "openai/gpt-4o-mini";
    public string SiteUrl { get; set; } = string.Empty;
    public string SiteName { get; set; } = string.Empty;
}
