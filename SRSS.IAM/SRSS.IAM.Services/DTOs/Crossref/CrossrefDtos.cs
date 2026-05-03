using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SRSS.IAM.Services.DTOs.Crossref;

// ─── Envelope ────────────────────────────────────────────────────────────────

public class CrossrefResponse<T>
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("message-type")]
    public string MessageType { get; set; } = string.Empty;

    [JsonPropertyName("message-version")]
    public string? MessageVersion { get; set; }

    [JsonPropertyName("message")]
    public T Message { get; set; } = default!;
}

// ─── List wrapper (GET /works) ────────────────────────────────────────────────

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

    /// <summary>Facet counts; present only when the <c>facet</c> query parameter is used.</summary>
    [JsonPropertyName("facets")]
    public System.Text.Json.JsonElement? Facets { get; set; }

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

// ─── Work (shared between singleton and list items) ───────────────────────────

public class CrossrefWorkDto
{
    // ── Core identifiers ────────────────────────────────────────────────────
    [JsonPropertyName("DOI")]
    public string Doi { get; set; } = string.Empty;

    [JsonPropertyName("URL")]
    public string? Url { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("subtype")]
    public string? Subtype { get; set; }

    [JsonPropertyName("source")]
    public string? Source { get; set; }

    [JsonPropertyName("member")]
    public string? Member { get; set; }

    [JsonPropertyName("prefix")]
    public string? Prefix { get; set; }

    // ── Bibliographic identifiers ────────────────────────────────────────────
    [JsonPropertyName("ISSN")]
    public List<string>? Issn { get; set; }

    [JsonPropertyName("issn-type")]
    public List<CrossrefIssnTypeDto>? IssnType { get; set; }

    [JsonPropertyName("ISBN")]
    public List<string>? Isbn { get; set; }

    [JsonPropertyName("isbn-type")]
    public List<CrossrefIsbnTypeDto>? IsbnType { get; set; }

    [JsonPropertyName("alternative-id")]
    public List<string>? AlternativeId { get; set; }

    [JsonPropertyName("article-number")]
    public string? ArticleNumber { get; set; }

    [JsonPropertyName("part-number")]
    public string? PartNumber { get; set; }

    [JsonPropertyName("edition-number")]
    public string? EditionNumber { get; set; }

    // ── Titles ───────────────────────────────────────────────────────────────
    [JsonPropertyName("title")]
    public List<string> Title { get; set; } = new();

    [JsonPropertyName("subtitle")]
    public List<string>? Subtitle { get; set; }

    [JsonPropertyName("short-title")]
    public List<string>? ShortTitle { get; set; }

    [JsonPropertyName("original-title")]
    public List<string>? OriginalTitle { get; set; }

    [JsonPropertyName("group-title")]
    public string? GroupTitle { get; set; }

    [JsonPropertyName("container-title")]
    public List<string>? ContainerTitle { get; set; }

    [JsonPropertyName("short-container-title")]
    public List<string>? ShortContainerTitle { get; set; }

    // ── People ───────────────────────────────────────────────────────────────
    [JsonPropertyName("author")]
    public List<CrossrefContributorDto>? Author { get; set; }

    [JsonPropertyName("editor")]
    public List<CrossrefContributorDto>? Editor { get; set; }

    [JsonPropertyName("chair")]
    public List<CrossrefContributorDto>? Chair { get; set; }

    [JsonPropertyName("translator")]
    public List<CrossrefContributorDto>? Translator { get; set; }

    // ── Publication details ───────────────────────────────────────────────────
    [JsonPropertyName("publisher")]
    public string? Publisher { get; set; }

    [JsonPropertyName("publisher-location")]
    public string? PublisherLocation { get; set; }

    [JsonPropertyName("abstract")]
    public string? Abstract { get; set; }

    [JsonPropertyName("language")]
    public string? Language { get; set; }

    [JsonPropertyName("volume")]
    public string? Volume { get; set; }

    [JsonPropertyName("issue")]
    public string? Issue { get; set; }

    [JsonPropertyName("page")]
    public string? Page { get; set; }

    [JsonPropertyName("subject")]
    public List<string>? Subject { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    // ── Dates ────────────────────────────────────────────────────────────────
    [JsonPropertyName("created")]
    public CrossrefDateDto? Created { get; set; }

    [JsonPropertyName("deposited")]
    public CrossrefDateDto? Deposited { get; set; }

    [JsonPropertyName("indexed")]
    public CrossrefIndexedDateDto? Indexed { get; set; }

    [JsonPropertyName("issued")]
    public CrossrefPartialDateDto? Issued { get; set; }

    [JsonPropertyName("published")]
    public CrossrefPartialDateDto? Published { get; set; }

    [JsonPropertyName("published-online")]
    public CrossrefPartialDateDto? PublishedOnline { get; set; }

    [JsonPropertyName("published-print")]
    public CrossrefPartialDateDto? PublishedPrint { get; set; }

    [JsonPropertyName("accepted")]
    public CrossrefPartialDateDto? Accepted { get; set; }

    [JsonPropertyName("approved")]
    public CrossrefPartialDateDto? Approved { get; set; }

    [JsonPropertyName("posted")]
    public CrossrefPartialDateDto? Posted { get; set; }

    [JsonPropertyName("content-created")]
    public CrossrefPartialDateDto? ContentCreated { get; set; }

    [JsonPropertyName("content-updated")]
    public CrossrefPartialDateDto? ContentUpdated { get; set; }

    // ── Metrics ───────────────────────────────────────────────────────────────
    [JsonPropertyName("is-referenced-by-count")]
    public int IsReferencedByCount { get; set; }

    [JsonPropertyName("references-count")]
    public int ReferencesCount { get; set; }

    [JsonPropertyName("reference-count")]
    public int ReferenceCount { get; set; }

    [JsonPropertyName("score")]
    public double? Score { get; set; }

    // ── Related objects ───────────────────────────────────────────────────────
    [JsonPropertyName("license")]
    public List<CrossrefLicenseDto>? License { get; set; }

    [JsonPropertyName("funder")]
    public List<CrossrefFunderDto>? Funder { get; set; }

    [JsonPropertyName("reference")]
    public List<CrossrefReferenceDto>? Reference { get; set; }

    [JsonPropertyName("link")]
    public List<CrossrefLinkDto>? Link { get; set; }

    [JsonPropertyName("resource")]
    public CrossrefResourceDto? Resource { get; set; }

    [JsonPropertyName("content-domain")]
    public CrossrefContentDomainDto? ContentDomain { get; set; }

    [JsonPropertyName("institution")]
    public List<CrossrefInstitutionDto>? Institution { get; set; }

    [JsonPropertyName("journal-issue")]
    public CrossrefJournalIssueDto? JournalIssue { get; set; }

    [JsonPropertyName("event")]
    public CrossrefEventDto? Event { get; set; }

    [JsonPropertyName("assertion")]
    public List<CrossrefAssertionDto>? Assertion { get; set; }

    [JsonPropertyName("update-to")]
    public List<CrossrefUpdateDto>? UpdateTo { get; set; }

    [JsonPropertyName("updated-by")]
    public List<CrossrefUpdateDto>? UpdatedBy { get; set; }

    [JsonPropertyName("update-policy")]
    public string? UpdatePolicy { get; set; }

    [JsonPropertyName("archive")]
    public List<string>? Archive { get; set; }

    [JsonPropertyName("degree")]
    public List<string>? Degree { get; set; }

    [JsonPropertyName("aliases")]
    public List<string>? Aliases { get; set; }

    [JsonPropertyName("issue-title")]
    public List<string>? IssueTitle { get; set; }

    [JsonPropertyName("special-numbering")]
    public string? SpecialNumbering { get; set; }

    [JsonPropertyName("component-number")]
    public string? ComponentNumber { get; set; }

    [JsonPropertyName("proceedings-subject")]
    public string? ProceedingsSubject { get; set; }

    [JsonPropertyName("standards-body")]
    public CrossrefStandardsBodyDto? StandardsBody { get; set; }
}

// ─── Contributor ─────────────────────────────────────────────────────────────

public class CrossrefContributorDto
{
    [JsonPropertyName("ORCID")]
    public string? Orcid { get; set; }

    [JsonPropertyName("authenticated-orcid")]
    public bool? AuthenticatedOrcid { get; set; }

    [JsonPropertyName("given")]
    public string? Given { get; set; }

    [JsonPropertyName("family")]
    public string? Family { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("suffix")]
    public string? Suffix { get; set; }

    [JsonPropertyName("prefix")]
    public string? Prefix { get; set; }

    [JsonPropertyName("sequence")]
    public string? Sequence { get; set; }

    [JsonPropertyName("affiliation")]
    public List<CrossrefAffiliationDto>? Affiliation { get; set; }
}

// ─── Affiliation ─────────────────────────────────────────────────────────────

public class CrossrefAffiliationDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("place")]
    public List<string>? Place { get; set; }

    [JsonPropertyName("department")]
    public List<string>? Department { get; set; }

    [JsonPropertyName("acronym")]
    public List<string>? Acronym { get; set; }

    [JsonPropertyName("id")]
    public List<CrossrefIdDto>? Id { get; set; }
}

// ─── Dates ───────────────────────────────────────────────────────────────────

/// <summary>Full date — date-parts + date-time + timestamp (e.g. created, deposited).</summary>
public class CrossrefDateDto
{
    [JsonPropertyName("date-parts")]
    public List<List<int?>> DateParts { get; set; } = new();

    [JsonPropertyName("date-time")]
    public string? DateTime { get; set; }

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }
}

/// <summary>Indexed date — adds a version string on top of the full date.</summary>
public class CrossrefIndexedDateDto : CrossrefDateDto
{
    [JsonPropertyName("version")]
    public string? Version { get; set; }
}

/// <summary>Partial date — date-parts only (e.g. published, issued, accepted).</summary>
public class CrossrefPartialDateDto
{
    [JsonPropertyName("date-parts")]
    public List<List<int?>>? DateParts { get; set; }
}

// ─── License ─────────────────────────────────────────────────────────────────

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

// ─── Funder ───────────────────────────────────────────────────────────────────

public class CrossrefFunderDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("DOI")]
    public string? Doi { get; set; }

    [JsonPropertyName("doi-asserted-by")]
    public string? DoiAssertedBy { get; set; }

    [JsonPropertyName("award")]
    public List<string>? Award { get; set; }

    [JsonPropertyName("id")]
    public List<CrossrefIdDto>? Id { get; set; }
}

// ─── Reference ───────────────────────────────────────────────────────────────

public class CrossrefReferenceDto
{
    [JsonPropertyName("key")]
    public string? Key { get; set; }

    [JsonPropertyName("DOI")]
    public string? Doi { get; set; }

    [JsonPropertyName("doi-asserted-by")]
    public string? DoiAssertedBy { get; set; }

    [JsonPropertyName("issn")]
    public string? Issn { get; set; }

    [JsonPropertyName("isbn-type")]
    public string? IsbnType { get; set; }

    [JsonPropertyName("isbn")]
    public string? Isbn { get; set; }

    [JsonPropertyName("issue")]
    public string? Issue { get; set; }

    [JsonPropertyName("volume")]
    public string? Volume { get; set; }

    [JsonPropertyName("first-page")]
    public string? FirstPage { get; set; }

    [JsonPropertyName("article-title")]
    public string? ArticleTitle { get; set; }

    [JsonPropertyName("volume-title")]
    public string? VolumeTitle { get; set; }

    [JsonPropertyName("series-title")]
    public string? SeriesTitle { get; set; }

    [JsonPropertyName("journal-title")]
    public string? JournalTitle { get; set; }

    [JsonPropertyName("standards-body")]
    public string? StandardsBody { get; set; }

    [JsonPropertyName("standard-designator")]
    public string? StandardDesignator { get; set; }

    [JsonPropertyName("author")]
    public string? Author { get; set; }

    [JsonPropertyName("year")]
    public string? Year { get; set; }

    [JsonPropertyName("edition")]
    public string? Edition { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("component")]
    public string? Component { get; set; }

    [JsonPropertyName("unstructured")]
    public string? Unstructured { get; set; }

    [JsonPropertyName("issn-type")]
    public string? IssnType { get; set; }
}

// ─── Link ─────────────────────────────────────────────────────────────────────

public class CrossrefLinkDto
{
    [JsonPropertyName("URL")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("content-type")]
    public string? ContentType { get; set; }

    [JsonPropertyName("content-version")]
    public string? ContentVersion { get; set; }

    [JsonPropertyName("intended-application")]
    public string? IntendedApplication { get; set; }
}

// ─── Resource ─────────────────────────────────────────────────────────────────

public class CrossrefResourceDto
{
    [JsonPropertyName("primary")]
    public CrossrefResourceUrlDto? Primary { get; set; }

    [JsonPropertyName("secondary")]
    public List<CrossrefResourceSecondaryDto>? Secondary { get; set; }
}

public class CrossrefResourceUrlDto
{
    [JsonPropertyName("URL")]
    public string Url { get; set; } = string.Empty;
}

public class CrossrefResourceSecondaryDto
{
    [JsonPropertyName("URL")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string? Label { get; set; }
}

// ─── Content domain ───────────────────────────────────────────────────────────

public class CrossrefContentDomainDto
{
    [JsonPropertyName("domain")]
    public List<string>? Domain { get; set; }

    [JsonPropertyName("crossmark-restriction")]
    public bool CrossmarkRestriction { get; set; }
}

// ─── Institution ─────────────────────────────────────────────────────────────

public class CrossrefInstitutionDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("place")]
    public List<string>? Place { get; set; }

    [JsonPropertyName("department")]
    public List<string>? Department { get; set; }

    [JsonPropertyName("acronym")]
    public List<string>? Acronym { get; set; }

    [JsonPropertyName("id")]
    public List<CrossrefIdDto>? Id { get; set; }
}

// ─── Journal issue ────────────────────────────────────────────────────────────

public class CrossrefJournalIssueDto
{
    [JsonPropertyName("issue")]
    public string? Issue { get; set; }

    [JsonPropertyName("published-online")]
    public CrossrefPartialDateDto? PublishedOnline { get; set; }

    [JsonPropertyName("published-print")]
    public CrossrefPartialDateDto? PublishedPrint { get; set; }
}

// ─── Event ───────────────────────────────────────────────────────────────────

public class CrossrefEventDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("start")]
    public CrossrefPartialDateDto? Start { get; set; }

    [JsonPropertyName("end")]
    public CrossrefPartialDateDto? End { get; set; }
}

// ─── Assertion ───────────────────────────────────────────────────────────────

public class CrossrefAssertionDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }

    [JsonPropertyName("label")]
    public string? Label { get; set; }

    [JsonPropertyName("URL")]
    public string? Url { get; set; }

    [JsonPropertyName("order")]
    public int? Order { get; set; }

    [JsonPropertyName("group")]
    public CrossrefAssertionGroupDto? Group { get; set; }

    [JsonPropertyName("explanation")]
    public CrossrefResourceUrlDto? Explanation { get; set; }
}

public class CrossrefAssertionGroupDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("label")]
    public string? Label { get; set; }
}

// ─── Update record ────────────────────────────────────────────────────────────

public class CrossrefUpdateDto
{
    [JsonPropertyName("label")]
    public string? Label { get; set; }

    [JsonPropertyName("DOI")]
    public string? Doi { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("source")]
    public string? Source { get; set; }

    [JsonPropertyName("record-id")]
    public string? RecordId { get; set; }

    [JsonPropertyName("updated")]
    public CrossrefDateDto? Updated { get; set; }
}

// ─── Standards body ───────────────────────────────────────────────────────────

public class CrossrefStandardsBodyDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("acronym")]
    public string? Acronym { get; set; }
}

// ─── ISSN / ISBN type ─────────────────────────────────────────────────────────

public class CrossrefIssnTypeDto
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }
}

public class CrossrefIsbnTypeDto
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }
}

// ─── Generic identifier ───────────────────────────────────────────────────────

public class CrossrefIdDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("id-type")]
    public string? IdType { get; set; }

    [JsonPropertyName("asserted-by")]
    public string? AssertedBy { get; set; }
}

// ─── Agency (GET /works/{doi}/agency) ────────────────────────────────────────

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
