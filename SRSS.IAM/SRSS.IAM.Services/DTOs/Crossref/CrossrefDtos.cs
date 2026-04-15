using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SRSS.IAM.Services.DTOs.Crossref;

public class CrossrefResponse<T>
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("message-type")]
    public string MessageType { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public T Message { get; set; } = default!;
}

public class CrossrefMessageList<T>
{
    [JsonPropertyName("items-per-page")]
    public int ItemsPerPage { get; set; }

    [JsonPropertyName("query")]
    public CrossrefQueryMeta? Query { get; set; }

    [JsonPropertyName("total-results")]
    public int TotalResults { get; set; }

    [JsonPropertyName("next-cursor")]
    public string? NextCursor { get; set; }

    [JsonPropertyName("items")]
    public List<T> Items { get; set; } = new();
}

public class CrossrefQueryMeta
{
    [JsonPropertyName("start-index")]
    public int StartIndex { get; set; }

    [JsonPropertyName("search-terms")]
    public string? SearchTerms { get; set; }
}

public class CrossrefWorkDto
{
    [JsonPropertyName("DOI")]
    public string Doi { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public List<string> Title { get; set; } = new();

    [JsonPropertyName("author")]
    public List<CrossrefAuthorDto> Author { get; set; } = new();

    [JsonPropertyName("publisher")]
    public string? Publisher { get; set; }

    [JsonPropertyName("is-referenced-by-count")]
    public int IsReferencedByCount { get; set; }

    [JsonPropertyName("created")]
    public CrossrefDateDto? Created { get; set; }

    [JsonPropertyName("published")]
    public CrossrefDateDto? Published { get; set; }

    [JsonPropertyName("license")]
    public List<CrossrefLicenseDto>? License { get; set; }

    [JsonPropertyName("funder")]
    public List<CrossrefFunderDto>? Funder { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}

public class CrossrefAuthorDto
{
    [JsonPropertyName("given")]
    public string? Given { get; set; }

    [JsonPropertyName("family")]
    public string? Family { get; set; }

    [JsonPropertyName("ORCID")]
    public string? Orcid { get; set; }

    [JsonPropertyName("authenticated-orcid")]
    public bool? AuthenticatedOrcid { get; set; }

    [JsonPropertyName("sequence")]
    public string? Sequence { get; set; }

    [JsonPropertyName("affiliation")]
    public List<CrossrefAffiliationDto>? Affiliation { get; set; }
}

public class CrossrefAffiliationDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class CrossrefDateDto
{
    [JsonPropertyName("date-parts")]
    public List<List<int>> DateParts { get; set; } = new();

    [JsonPropertyName("date-time")]
    public string? DateTime { get; set; }

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }
}

public class CrossrefLicenseDto
{
    [JsonPropertyName("URL")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("start")]
    public CrossrefDateDto? Start { get; set; }

    [JsonPropertyName("delay-in-days")]
    public int DelayInDays { get; set; }

    [JsonPropertyName("content-version")]
    public string? ContentVersion { get; set; }
}

public class CrossrefFunderDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("DOI")]
    public string? Doi { get; set; }

    [JsonPropertyName("award")]
    public List<string>? Award { get; set; }

    [JsonPropertyName("doi-asserted-by")]
    public string? DoiAssertedBy { get; set; }
}

public class CrossrefAgencyDto
{
    [JsonPropertyName("DOI")]
    public string Doi { get; set; } = string.Empty;

    [JsonPropertyName("agency")]
    public CrossrefAgencyInfo? Agency { get; set; }
}

public class CrossrefAgencyInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;
}
