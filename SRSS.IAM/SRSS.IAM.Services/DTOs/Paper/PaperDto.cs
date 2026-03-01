using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Services.DTOs.Paper
{
    public class PaperResponse
    {
        public Guid Id { get; set; }

        // Core Metadata
        public string Title { get; set; } = string.Empty;
        public string? Authors { get; set; }
        public string? Abstract { get; set; }
        public string? DOI { get; set; }
        public string? PublicationType { get; set; }
        public string? PublicationYear { get; set; }
        public int? PublicationYearInt { get; set; }
        public DateTimeOffset? PublicationDate { get; set; }
        public string? Volume { get; set; }
        public string? Issue { get; set; }
        public string? Pages { get; set; }
        public string? Publisher { get; set; }
        public string? Language { get; set; }
        public string? Keywords { get; set; }
        public string? Url { get; set; }

        // Conference Metadata
        public string? ConferenceName { get; set; }
        public string? ConferenceLocation { get; set; }
        public string? ConferenceCountry { get; set; }
        public int? ConferenceYear { get; set; }

        // Journal Metadata
        public string? Journal { get; set; }
        public string? JournalIssn { get; set; }

        // Source Tracking
        public string? Source { get; set; }
        public DateTimeOffset? ImportedAt { get; set; }
        public string? ImportedBy { get; set; }

        // Selection Status (derived dynamically from ScreeningResolution)
        public SelectionStatus? SelectionStatus { get; set; }
        public string? SelectionStatusText { get; set; }

        // Access
        public string? PdfUrl { get; set; }
        public bool? FullTextAvailable { get; set; }
        public AccessType? AccessType { get; set; }
        public string? AccessTypeText { get; set; }

        // Audit
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset ModifiedAt { get; set; }
    }

    public class PaperListRequest
    {
        public string? Search { get; set; }
        public SelectionStatus? Status { get; set; }
        public int? Year { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class DuplicatePapersRequest
    {
        public string? Search { get; set; }
        public int? Year { get; set; }
        public string? SortBy { get; set; }
        public string? SortOrder { get; set; }
        public DeduplicationReviewStatus? ReviewStatus { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// Response that includes deduplication metadata
    /// </summary>
    public class DuplicatePaperResponse : PaperResponse
    {
        // Deduplication metadata
        public Guid DuplicateOfPaperId { get; set; }
        public string? DuplicateOfTitle { get; set; }
        public string? DuplicateOfAuthors { get; set; }
        public string? DuplicateOfYear { get; set; }
        public string? DuplicateOfDoi { get; set; }
        public string? DuplicateOfSource { get; set; }
        public string? DuplicateOfAbstract { get; set; }
        public DeduplicationMethod Method { get; set; }
        public string MethodText { get; set; } = string.Empty;
        public decimal? ConfidenceScore { get; set; }
        public string? DeduplicationNotes { get; set; }
        public DateTimeOffset DetectedAt { get; set; }
        public DeduplicationReviewStatus ReviewStatus { get; set; } = DeduplicationReviewStatus.Pending;
        public string ReviewStatusText { get; set; } = DeduplicationReviewStatus.Pending.ToString();
        public string? ReviewedBy { get; set; }
        public DateTimeOffset? ReviewedAt { get; set; }
    }

    /// <summary>
    /// Request to resolve a duplicate detection result
    /// </summary>
    public class ResolveDuplicateRequest
    {
        public DeduplicationReviewStatus Resolution { get; set; }
        public string? ReviewedBy { get; set; }
        public string? Notes { get; set; }
    }
}
