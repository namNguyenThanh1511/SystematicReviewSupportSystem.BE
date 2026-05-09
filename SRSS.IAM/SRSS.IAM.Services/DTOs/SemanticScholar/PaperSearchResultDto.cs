namespace SRSS.IAM.Services.DTOs.SemanticScholar;

public class PaperSearchResultDto
{
    public string PaperId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public List<string> Authors { get; set; } = new();
    public string? Abstract { get; set; }
    public int? Year { get; set; }
    public string? Doi { get; set; }
    public string? Journal { get; set; }
    public string? Url { get; set; }
    public string? OpenAccessPdfUrl { get; set; }
    public string PdfStatus { get; set; } = "UNKNOWN";

    public static string ClassifyPdfStatus(string? url)
    {
        if (string.IsNullOrEmpty(url)) return "NONE";

        // ArXiv is always trusted as AVAILABLE
        if (url.Contains("arxiv.org", System.StringComparison.OrdinalIgnoreCase)) return "AVAILABLE";

        // Any other OA PDF is likely available
        return "LIKELY_AVAILABLE";
    }
}
