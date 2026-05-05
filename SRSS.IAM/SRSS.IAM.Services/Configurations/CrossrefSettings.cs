namespace SRSS.IAM.Services.Configurations;

public class CrossrefSettings
{
    public const string SectionName = "Crossref";
    public string BaseUrl { get; set; } = "https://api.crossref.org/";
    public string UserAgent { get; set; } = "SRSS-App/1.0 (mailto:admin@example.com)";

    /// <summary>
    /// Contact e-mail appended as <c>mailto</c> query parameter on every request.
    /// This grants access to Crossref's "polite pool" with higher rate limits.
    /// </summary>
    public string? Mailto { get; set; }
}
