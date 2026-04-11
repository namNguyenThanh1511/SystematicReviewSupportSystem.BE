namespace SRSS.IAM.Services.DTOs.Crossref;

public class CrossrefQueryParameters
{
    public string? Query { get; set; }
    public string? QueryAuthor { get; set; }
    public string? QueryTitle { get; set; }
    public string? QueryBibliographic { get; set; }
    public string? Filter { get; set; }
    public string? Sort { get; set; }
    public string? Order { get; set; }
    public string? Facet { get; set; }
    public string? Select { get; set; }
    public int? Rows { get; set; }
    public int? Offset { get; set; }
    public string? Cursor { get; set; }
    public int? Sample { get; set; }
}
