namespace SRSS.IAM.Services.Configurations;

public class CrossrefSettings
{
    public const string SectionName = "Crossref";
    public string BaseUrl { get; set; } = "https://api.crossref.org/";
    public string UserAgent { get; set; } = "SRSS-App/1.0 (mailto:admin@example.com)";
}
