using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Services.DTOs.Paper
{
    /// <summary>
    /// Lightweight paper DTO for side-by-side duplicate comparison.
    /// Contains only fields relevant for researcher review.
    /// </summary>
    public class DuplicatePairPaperDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Authors { get; set; }
        public string? Abstract { get; set; }
        public string? DOI { get; set; }
        public string? PublicationType { get; set; }
        public string? PublicationYear { get; set; }
        public int? PublicationYearInt { get; set; }
        public string? Source { get; set; }
        public string? Journal { get; set; }
        public string? Keywords { get; set; }
        public string? Url { get; set; }
        public DateTimeOffset? ImportedAt { get; set; }
    }

    /// <summary>
    /// A duplicate pair: both the original and duplicate paper with full comparison metadata.
    /// </summary>
    public class DuplicatePairResponse
    {
        public Guid Id { get; set; }
        public DuplicatePairPaperDto OriginalPaper { get; set; } = null!;
        public DuplicatePairPaperDto DuplicatePaper { get; set; } = null!;
        public DeduplicationMethod Method { get; set; }
        public string MethodText { get; set; } = string.Empty;
        public decimal? ConfidenceScore { get; set; }
        public string? DeduplicationNotes { get; set; }
        public required string ResolvedDecision { get; set; } 
        public DeduplicationReviewStatus ReviewStatus { get; set; } = DeduplicationReviewStatus.Pending;
        public string ReviewStatusText { get; set; } = DeduplicationReviewStatus.Pending.ToString();
        public string? ReviewedBy { get; set; }
        public DateTimeOffset? ReviewedAt { get; set; }
        public DateTimeOffset DetectedAt { get; set; }
    }

    /// <summary>
    /// Request parameters for the duplicate-pairs endpoint.
    /// </summary>
    public class DuplicatePairsRequest
    {
        public string? Search { get; set; }
        public DeduplicationReviewStatus? Status { get; set; }
        public decimal? MinConfidence { get; set; }
        public DeduplicationMethod? Method { get; set; }
        public string? SortBy { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// Request to resolve a duplicate pair decision.
    /// </summary>
    public class ResolveDuplicatePairRequest
    {
        public string Decision { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Response after resolving a duplicate pair.
    /// </summary>
    public class ResolveDuplicatePairResponse
    {
        public Guid Id { get; set; }
        public DeduplicationReviewStatus ReviewStatus { get; set; }
        public string ReviewStatusText { get; set; } = string.Empty;
        public string? ResolvedDecision { get; set; }
        public DateTimeOffset? ReviewedAt { get; set; }
        public string? ReviewedBy { get; set; }
    }
}
