using SRSS.IAM.Services.DTOs.Crossref;
using SRSS.IAM.Services.DTOs.Identification;

namespace SRSS.IAM.Services.Mappers;

/// <summary>
/// Maps Crossref API DTOs to the unified <see cref="RisPaperDto"/> used throughout the pipeline.
/// </summary>
public static class CrossrefMapper
{
    /// <summary>
    /// Maps a single <see cref="CrossrefWorkDto"/> to a <see cref="RisPaperDto"/>.
    /// </summary>
    public static RisPaperDto ToRisPaper(CrossrefWorkDto work)
    {
        var dto = new RisPaperDto
        {
            Title          = ExtractTitle(work),
            DOI            = NormalizeDoi(work.Doi),
            Publisher      = work.Publisher,
            PublicationType = MapPublicationType(work.Type),
            Url            = work.Url ?? BuildDoiUrl(work.Doi),
            Abstract       = work.Abstract,
            Volume         = work.Volume,
            Issue          = work.Issue,
            Pages          = work.Page,
            Journal        = work.ContainerTitle?.FirstOrDefault(),
            ConferenceName = work.Event?.Name,
            ConferenceLocation = work.Event?.Location,
        };

        // Authors: "Family, Given" joined as semicolon list
        dto.AuthorList.AddRange(ExtractAuthors(work));

        // Keywords / subjects
        if (work.Subject != null)
            dto.KeywordList.AddRange(work.Subject);

        // ISSN → JournalIssn
        dto.JournalIssn = work.Issn?.FirstOrDefault();

        // Publication year — prefer published (partial date), fall back to created (full date)
        var dateParts = work.Published?.DateParts
                     ?? work.PublishedOnline?.DateParts
                     ?? work.PublishedPrint?.DateParts
                     ?? work.Issued?.DateParts
                     ?? work.Created?.DateParts;

        if (dateParts is { Count: > 0 })
        {
            var parts = dateParts[0];
            if (parts.Count >= 1)
            {
                dto.PublicationYear = parts[0].ToString();

                if (parts.Count >= 3)
                {
                    var year  = parts[0];
                    var month = parts[1];
                    var day   = parts[2];

                    if (year.HasValue && month.HasValue && day.HasValue &&
                        month >= 1 && month <= 12 && day >= 1 && day <= 31)
                    {
                        dto.PublicationDate = new DateTimeOffset(
                            new DateTime(year.Value, month.Value, day.Value, 0, 0, 0, DateTimeKind.Utc));
                    }
                }
            }
        }

        // Conference date
        if (work.Event?.Start?.DateParts is { Count: > 0 })
        {
            var startParts = work.Event.Start.DateParts[0];
            dto.ConferenceDate = string.Join("-", startParts);
        }

        // License URL as a supplementary URL when no DOI URL is set
        var licenseUrl = work.License?.FirstOrDefault()?.Url;
        if (!string.IsNullOrWhiteSpace(licenseUrl) && string.IsNullOrWhiteSpace(dto.Url))
            dto.Url = licenseUrl;

        // Raw reference for deduplication / auditing
        dto.RawReference = BuildRawReference(work);

        return dto;
    }

    // ─── Private helpers ───────────────────────────────────────────────────────

    private static string ExtractTitle(CrossrefWorkDto work)
    {
        var title = work.Title?.FirstOrDefault(t => !string.IsNullOrWhiteSpace(t));
        return title ?? string.Empty;
    }

    private static IEnumerable<string> ExtractAuthors(CrossrefWorkDto work)
    {
        if (work.Author == null || work.Author.Count == 0)
            yield break;

        foreach (var author in work.Author)
        {
            var parts = new List<string>(2);
            if (!string.IsNullOrWhiteSpace(author.Family)) parts.Add(author.Family!.Trim());
            if (!string.IsNullOrWhiteSpace(author.Given))  parts.Add(author.Given!.Trim());

            if (parts.Count > 0)
                yield return string.Join(", ", parts);
        }
    }

    private static string? NormalizeDoi(string? rawDoi)
    {
        if (string.IsNullOrWhiteSpace(rawDoi)) return null;

        // Strip any leading "https://doi.org/" or "http://dx.doi.org/" prefix
        rawDoi = rawDoi.Trim();
        foreach (var prefix in new[] { "https://doi.org/", "http://doi.org/", "https://dx.doi.org/", "http://dx.doi.org/" })
        {
            if (rawDoi.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return rawDoi[prefix.Length..].Trim();
        }

        return rawDoi;
    }

    private static string? BuildDoiUrl(string? doi)
    {
        var normalized = NormalizeDoi(doi);
        return string.IsNullOrWhiteSpace(normalized) ? null : $"https://doi.org/{normalized}";
    }

    /// <summary>
    /// Maps Crossref work type strings to simpler publication type labels.
    /// </summary>
    private static string? MapPublicationType(string? type)
    {
        return type?.ToLowerInvariant() switch
        {
            "journal-article"       => "JOUR",
            "proceedings-article"   => "CONF",
            "book-chapter"          => "CHAP",
            "book"                  => "BOOK",
            "report"                => "RPRT",
            "dissertation"          => "THES",
            "dataset"               => "DATA",
            "preprint"              => "UNPB",
            _                       => type
        };
    }

    /// <summary>
    /// Builds a minimal human-readable raw reference string for audit / deduplication.
    /// </summary>
    private static string BuildRawReference(CrossrefWorkDto work)
    {
        var authors = ExtractAuthors(work);
        var authorStr = string.Join("; ", authors);
        var title = ExtractTitle(work);

        var yearParts = (work.Published?.DateParts
                      ?? work.PublishedOnline?.DateParts
                      ?? work.PublishedPrint?.DateParts
                      ?? work.Issued?.DateParts
                      ?? work.Created?.DateParts)
                      ?.FirstOrDefault();
        var year = yearParts?.FirstOrDefault()?.ToString() ?? "n.d.";

        var doi = NormalizeDoi(work.Doi);

        return $"{authorStr} ({year}). {title}. DOI: {doi}".Trim();
    }
}
