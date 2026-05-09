using System.Text.Json.Serialization;

namespace SRSS.IAM.Services.DTOs.SemanticScholar;

public class SemanticScholarPaperDto
{
    [JsonPropertyName("paperId")]
    public string PaperId { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("authors")]
    public List<SemanticScholarAuthorDto>? Authors { get; set; }

    [JsonPropertyName("year")]
    public int? Year { get; set; }

    [JsonPropertyName("abstract")]
    public string? Abstract { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("externalIds")]
    public SemanticScholarExternalIdsDto? ExternalIds { get; set; }

    [JsonPropertyName("journal")]
    public SemanticScholarJournalDto? Journal { get; set; }

    [JsonPropertyName("publicationTypes")]
    public List<string>? PublicationTypes { get; set; }

    [JsonPropertyName("openAccessPdf")]
    public SemanticScholarOpenAccessPdfDto? OpenAccessPdf { get; set; }
}

public class SemanticScholarAuthorDto
{
    [JsonPropertyName("authorId")]
    public string? AuthorId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class SemanticScholarExternalIdsDto
{
    [JsonPropertyName("DOI")]
    public string? DOI { get; set; }

    [JsonPropertyName("PubMed")]
    public string? PubMed { get; set; }

    [JsonPropertyName("ArXiv")]
    public string? ArXiv { get; set; }
}

public class SemanticScholarJournalDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("pages")]
    public string? Pages { get; set; }

    [JsonPropertyName("volume")]
    public string? Volume { get; set; }
}

public class SemanticScholarOpenAccessPdfDto
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }
}

public class SemanticScholarSearchApiResponse
{
    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("offset")]
    public int Offset { get; set; }

    [JsonPropertyName("data")]
    public List<SemanticScholarPaperDto> Data { get; set; } = new();
}
