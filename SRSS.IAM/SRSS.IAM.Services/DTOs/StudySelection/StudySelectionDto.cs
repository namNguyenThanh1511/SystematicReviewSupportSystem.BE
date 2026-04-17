using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;

namespace SRSS.IAM.Services.DTOs.StudySelection
{
    // ============================================
    // REQUEST DTOs
    // ============================================

    public class CreateStudySelectionProcessRequest
    {
        public Guid ReviewProcessId { get; set; }
        public string? Notes { get; set; }
    }

    public class SubmitScreeningDecisionRequest
    {
        public Guid ReviewerId { get; set; }
        public ScreeningDecisionType Decision { get; set; }
        public ScreeningPhase Phase { get; set; }
        public string? Reason { get; set; }
        public Guid? ExclusionReasonId { get; set; }
    }

    public class UpdateScreeningDecisionRequest
    {
        public ScreeningDecisionType Decision { get; set; }
        public string? Reason { get; set; }
        public Guid? ExclusionReasonId { get; set; }
        public Guid? ChecklistSubmissionId { get; set; }
    }

    public class ResolveScreeningConflictRequest
    {
        public ScreeningDecisionType FinalDecision { get; set; }
        public ScreeningPhase Phase { get; set; }
        public Guid ResolvedBy { get; set; }
        public Guid? ExclusionReasonId { get; set; }
        public string? ResolutionNotes { get; set; }
    }

    public class BulkResolveConflictsRequest
    {
        public List<Guid> PaperIds { get; set; } = new();
        public ScreeningDecisionType FinalDecision { get; set; }
        public ScreeningPhase Phase { get; set; }
        public Guid ResolvedBy { get; set; }
        public Guid? ExclusionReasonId { get; set; }
        public string? ResolutionNotes { get; set; }
    }


    public class GetResolutionsRequest
    {
        public ScreeningPhase? Phase { get; set; }
        public ResolutionFilterStatus Status { get; set; } = ResolutionFilterStatus.All;
        public string? Search { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class PapersWithDecisionsRequest
    {
        public string? Search { get; set; }
        public PaperSelectionStatus? Status { get; set; }
        public PaperSortBy SortBy { get; set; } = PaperSortBy.TitleAsc;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        // Issue 4 filters
        public bool? HasFullText { get; set; }
        public bool? HasConflict { get; set; }
        public Guid? DecidedByReviewerId { get; set; }
        /// <summary>
        /// Screening phase filter. TitleAbstract = all eligible papers, FullText = only TA-included papers.
        /// When null, defaults to TitleAbstract behavior.
        /// </summary>
        public ScreeningPhase? Phase { get; set; }
    }

    /// <summary>
    /// Issue 2: Update full-text link or upload for a paper
    /// </summary>
    public class UpdatePaperFullTextRequest
    {
        /// <summary>External URL to the PDF (mutually exclusive with file upload)</summary>
        public string? PdfUrl { get; set; }
        /// <summary>Original uploaded PDF filename</summary>
        public string? PdfFileName { get; set; }
        /// <summary>External URL to the web source</summary>
        public string? Url { get; set; }

        /// <summary>Whether to extract header metadata using GROBID</summary>
        public bool ExtractWithGrobid { get; set; }

        /// <summary>PDF file stream for GROBID extraction</summary>
        public System.IO.Stream? PdfStream { get; set; }
    }

    // ============================================
    // RESPONSE DTOs
    // ============================================

    public class StudySelectionProcessResponse
    {
        public Guid Id { get; set; }
        public Guid ReviewProcessId { get; set; }
        public string? Notes { get; set; }
        public DateTimeOffset? StartedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
        public SelectionProcessStatus Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset ModifiedAt { get; set; }
        [Obsolete("Use PhaseStatistics instead for phase-specific counts.")]
        public SelectionStatisticsResponse SelectionStatistics { get; set; }
        public PhaseStatisticsResponse PhaseStatistics { get; set; } = new();
        public TitleAbstractScreeningResponse? TitleAbstractScreening { get; set; }
    }

    public class StudySelectionPhaseStatusResponse
    {
        public ScreeningPhase CurrentPhase { get; set; }
        public string CurrentPhaseText { get; set; } = string.Empty;
        public bool TitleAbstractStarted { get; set; }
        public bool TitleAbstractCompleted { get; set; }
        public bool FullTextStarted { get; set; }
        public bool FullTextCompleted { get; set; }
    }

    public class TitleAbstractScreeningResponse
    {
        public Guid Id { get; set; }
        public Guid StudySelectionProcessId { get; set; }
        public ScreeningPhaseStatus Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public DateTimeOffset? StartedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
        public int MinReviewersPerPaper { get; set; }
        public int MaxReviewersPerPaper { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset ModifiedAt { get; set; }
    }

    public class ReviewerAssignmentResponse
    {
        public Guid ProjectMemberId { get; set; }
        public Guid ReviewerId { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
    }

    public class ScreeningDecisionResponse
    {
        public Guid Id { get; set; }
        public Guid StudySelectionProcessId { get; set; }
        public Guid PaperId { get; set; }
        public string PaperTitle { get; set; } = string.Empty;
        public Guid ReviewerId { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
        public ScreeningDecisionType Decision { get; set; }
        public string DecisionText { get; set; } = string.Empty;
        public ScreeningPhase Phase { get; set; }
        public string PhaseText { get; set; } = string.Empty;
        public Guid? ExclusionReasonId { get; set; }
        public int? ExclusionReasonCode { get; set; }
        public string? ExclusionReasonName { get; set; }
        public string? Reason { get; set; }
        public DateTimeOffset DecidedAt { get; set; }
    }

    public class ScreeningResolutionResponse
    {
        public Guid Id { get; set; }
        public Guid StudySelectionProcessId { get; set; }
        public Guid PaperId { get; set; }
        public string PaperTitle { get; set; } = string.Empty;
        public ScreeningDecisionType FinalDecision { get; set; }
        public string FinalDecisionText { get; set; } = string.Empty;
        public ScreeningPhase Phase { get; set; }
        public string PhaseText { get; set; } = string.Empty;
        public Guid? ExclusionReasonId { get; set; }
        public int? ExclusionReasonCode { get; set; }
        public string? ExclusionReasonName { get; set; }
        public string? ResolutionNotes { get; set; }
        public Guid ResolvedBy { get; set; }
        public string ResolverName { get; set; } = string.Empty;
        public DateTimeOffset ResolvedAt { get; set; }
    }

    public class ScreeningResolutionPaperResponse : ScreeningResolutionResponse
    {
        public string? Authors { get; set; }
        public string? DOI { get; set; }
        public string? PublicationYear { get; set; }
        public string? Source { get; set; }
    }

    public class PaperWithDecisionsResponse
    {
        public Guid PaperId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? DOI { get; set; }
        public string? Authors { get; set; }
        public string? PublicationYear { get; set; }
        public string? PublicationDate { get; set; }
        public string? Abstract { get; set; }
        public string? Journal { get; set; }
        public string? Source { get; set; }
        public string? Keywords { get; set; }
        public string? PublicationType { get; set; }
        public string? Volume { get; set; }
        public string? Issue { get; set; }
        public string? Pages { get; set; }
        public string? Publisher { get; set; }
        public string? Language { get; set; }
        public string? Url { get; set; }
        public string? PdfUrl { get; set; }
        public string? PdfFileName { get; set; }
        public string? ConferenceName { get; set; }
        public string? ConferenceLocation { get; set; }
        public string? JournalIssn { get; set; }
        public string? JournalEIssn { get; set; }
        public string? Md5 { get; set; }
        public FullTextRetrievalStatus FullTextRetrievalStatus { get; set; }
        public PaperSelectionStatus Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
        /// <summary>
        /// Issue 5: Deterministic final decision. Non-null when status is Included/Excluded/Resolved.
        /// Derived from resolution (if exists) or unanimous decisions.
        /// </summary>
        public ScreeningDecisionType? FinalDecision { get; set; }
        public string? FinalDecisionText { get; set; }

        public int CitationCount { get; set; }
        public int ReferenceCount { get; set; }

        public List<ScreeningDecisionResponse> Decisions { get; set; } = new();
        public ScreeningResolutionResponse? Resolution { get; set; }

        public ExtractionStatusResponse? Extraction { get; set; }
        public MetadataSourcesResponse? MetadataSources { get; set; }
        public ExtractionResultResponse? ExtractionResult { get; set; }
        public ExtractionSuggestionResponse? ExtractionSuggestion { get; set; }
    }

    public class ExtractionSuggestionResponse
    {
        public Guid SourceMetadataId { get; set; }
        public required Guid PaperId { get; set; }
        public string? Title { get; set; }
        public string? Authors { get; set; }
        public string? Abstract { get; set; }
        public string? DOI { get; set; }
        public string? Journal { get; set; }
        public string? Volume { get; set; }
        public string? Issue { get; set; }
        public string? Pages { get; set; }
        public string? Keywords { get; set; }
        public string? Language { get; set; }
        public string? Publisher { get; set; }
        public int? Year { get; set; }
        public string? ISSN { get; set; }
        public string? EISSN { get; set; }
        public string? Md5 { get; set; }
        public List<string> UpdatedFields { get; set; } = new();
    }

    public class ApplyMetadataRequest
    {
        public Guid SourceMetadataId { get; set; }
        public List<string> Fields { get; set; } = new();
    }

    public class ExtractionStatusResponse
    {
        public bool Requested { get; set; }
        public string? Provider { get; set; }
        public string Status { get; set; } = "not_requested"; // not_requested, succeeded, failed, partial
        public string? Message { get; set; }
        public string? RetryToken { get; set; }
    }

    public class MetadataSourcesResponse
    {
        public string? Title { get; set; }
        public string? Authors { get; set; }
        public string? Abstract { get; set; }
        public string? DOI { get; set; }
        public string? Journal { get; set; }
        public string? Volume { get; set; }
        public string? Issue { get; set; }
        public string? Pages { get; set; }
        public string? Keywords { get; set; }
        public string? Publisher { get; set; }
        public string? PublishedDate { get; set; }
        public string? Year { get; set; }
        public string? ISSN { get; set; }
        public string? EISSN { get; set; }
        public string? Language { get; set; }
        public string? Md5 { get; set; }
    }

    public class ExtractionResultResponse
    {
        public string? Title { get; set; }
        public string? Authors { get; set; }
        public string? Abstract { get; set; }
        public string? DOI { get; set; }
        public string? Journal { get; set; }
        public string? Volume { get; set; }
        public string? Issue { get; set; }
        public string? Pages { get; set; }
        public string? Keywords { get; set; }
        public string? Publisher { get; set; }
        public string? PublishedDate { get; set; }
        public int? Year { get; set; }
        public string? ISSN { get; set; }
        public string? EISSN { get; set; }
        public string? Language { get; set; }
        public string? Md5 { get; set; }
        public List<string> UpdatedFields { get; set; } = new();
    }

    public class ConflictPaperDetailResponse : PaperWithDecisionsResponse
    {
        public bool IsFinishReview { get; set; }
        public List<ReviewerAssignmentResponse> AssignedMembers { get; set; } = new();
    }

    public class ReviewerDecisionDetailResponse
    {
        public Guid ReviewerId { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
        public ScreeningDecisionResponse? Decision { get; set; }
    }

    public class ConflictedPaperResponse
    {
        public Guid PaperId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? DOI { get; set; }
        public List<ScreeningDecisionResponse> ConflictingDecisions { get; set; } = new();
    }

    public class PhaseConflictedPaperResponse
    {
        public Guid PaperId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Authors { get; set; }
        public string? DOI { get; set; }
        public string? Year { get; set; }
        public string? Source { get; set; }
        public ScreeningPhase Phase { get; set; }
        public string PhaseText { get; set; } = string.Empty;
        public PaperSelectionStatus Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
    }

    public class ConflictedPapersRequest
    {
        public ScreeningPhase? Phase { get; set; }
        public PaperSelectionStatus? Status { get; set; }
        public string? Search { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class SelectionStatisticsResponse
    {
        public Guid StudySelectionProcessId { get; set; }
        public int TotalPapers { get; set; }
        public int IncludedCount { get; set; }
        public int ExcludedCount { get; set; }
        public int ConflictCount { get; set; }
        public int PendingCount { get; set; }
        public double CompletionPercentage { get; set; }
        public List<ExclusionReasonBreakdownItem> ExclusionReasonBreakdown { get; set; } = new();
    }

    public class PhaseStatisticsResponse
    {
        public SelectionStatisticsResponse TitleAbstract { get; set; } = new();
        public SelectionStatisticsResponse FullText { get; set; } = new();
    }

    public class ExclusionReasonBreakdownItem
    {
        public int ReasonCode { get; set; }
        public string ReasonText { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class IncludedPaperResponse
    {
        public Guid PaperId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? DOI { get; set; }
        public string? Authors { get; set; }
        public string? Abstract { get; set; }
        public string? PublicationYear { get; set; }
        public string? PublicationType { get; set; }
        public string? Journal { get; set; }
        public string? Source { get; set; }
        public string? Keywords { get; set; }
        public string? Url { get; set; }
        public string? PdfUrl { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }

    public class DatasetPaperResponse
    {
        public Guid PaperId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Authors { get; set; }
        public string? PublicationYear { get; set; }
        public string? Domain { get; set; }
        public string? Abstract { get; set; }
    }

    /// <summary>
    /// Request to save multiple included papers in FullText phase
    /// </summary>
    public class SaveMultipleIncludedPapersRequest
    {
        public List<Guid> PaperIds { get; set; } = new();
    }

    // ============================================
    // ENUMS
    // ============================================

    public enum PaperSelectionStatus
    {
        Pending = 0,
        Included = 1,
        Excluded = 2,
        Conflict = 3,
        Resolved = 4
    }

    public enum PaperSortBy
    {
        TitleAsc = 0,
        TitleDesc = 1,
        YearNewest = 2,
        YearOldest = 3,
        /// <summary>Issue 3: Sort by decision count descending (most-reviewed first)</summary>
        RelevanceDesc = 4
    }
}

