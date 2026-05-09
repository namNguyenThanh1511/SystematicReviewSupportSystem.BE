namespace SRSS.IAM.Services.DTOs.SemanticScholar;

public class SemanticScholarSearchRequest
{
    public string Keyword { get; set; } = string.Empty;
    public int? YearFrom { get; set; }
    public int? YearTo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
