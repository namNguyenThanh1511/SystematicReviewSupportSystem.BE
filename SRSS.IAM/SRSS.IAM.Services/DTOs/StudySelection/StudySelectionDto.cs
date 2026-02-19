using SRSS.IAM.Repositories.Entities;

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
        public string? Reason { get; set; }
    }

    public class ResolveScreeningConflictRequest
    {
        public ScreeningDecisionType FinalDecision { get; set; }
        public Guid ResolvedBy { get; set; }
        public string? ResolutionNotes { get; set; }
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
        public int ScreenedStudy { get; set; }

        public int StudyToScreen { get; set; }
    }

    public class ScreeningDecisionResponse
    {
        public Guid Id { get; set; }
        public Guid StudySelectionProcessId { get; set; }
        public Guid PaperId { get; set; }
        public string PaperTitle { get; set; } = string.Empty;
        public Guid ReviewerId { get; set; }
        public ScreeningDecisionType Decision { get; set; }
        public string DecisionText { get; set; } = string.Empty;
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
        public string? ResolutionNotes { get; set; }
        public Guid ResolvedBy { get; set; }
        public DateTimeOffset ResolvedAt { get; set; }
    }

    public class PaperWithDecisionsResponse
    {
        public Guid PaperId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? DOI { get; set; }
        public string? Authors { get; set; }
        public int? PublicationYear { get; set; }
        public PaperSelectionStatus Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public List<ScreeningDecisionResponse> Decisions { get; set; } = new();
        public ScreeningResolutionResponse? Resolution { get; set; }
    }

    public class ConflictedPaperResponse
    {
        public Guid PaperId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? DOI { get; set; }
        public List<ScreeningDecisionResponse> ConflictingDecisions { get; set; } = new();
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
}
