using System.Text.Json.Serialization;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;
using SRSS.IAM.Services.DTOs.Common;

using SRSS.IAM.Services.DTOs.StudySelection;

namespace SRSS.IAM.Services.DTOs.QualityAssessment
{
    public class QualityAssessmentProcessResponse
    {
        public Guid Id { get; set; }
        public Guid ReviewProcessId { get; set; }
        public string? Notes { get; set; }
        public QualityAssessmentProcessStatus Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public DateTimeOffset? StartedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset ModifiedAt { get; set; }
        
        public QualityAssessmentStatisticsResponse? QualityStatistics { get; set; }
        public bool IsHaveCriteria { get; set; }
    }

    public class QualityAssessmentStatisticsResponse
    {
        public int TotalPapers { get; set; }
        public int HighQualityPapers { get; set; }
        public int LowQualityPapers { get; set; }
        public int InProgressPapers { get; set; }
        public int NotStartedPapers { get; set; }
        public bool HasSetupCriteria { get; set; }
    }

    public class CreateQualityAssessmentProcessDto
    {
        public Guid ReviewProcessId { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateQualityAssessmentProcessRequest
    {
        public string? Notes { get; set; }
        public QualityAssessmentProcessStatus Status { get; set; }
    }

    public class QualityAssessmentAssignmentRequest
    {
        public Guid? Id { get; set; }
        public Guid QualityAssessmentProcessId { get; set; }
        public Guid UserId { get; set; }
        public string? UserName { get; set; }
        public DateTimeOffset AssignedAt { get; set; }
        public List<QAPaperResponse>? Papers { get; set; }
        public List<QualityAssessmentDecisionResponse>? Decisions { get; set; }
    }

    public class QualityAssessmentDecisionResponse
    {
        public Guid? Id { get; set; }
        public Guid ReviewerId { get; set; }
        public string? ReviewerName { get; set; }
        public Guid PaperId { get; set; }
        public decimal? Score { get; set; }
        public List<QualityAssessmentDecisionItemResponse> DecisionItems { get; set; } = new();
    }

    public class QualityAssessmentDecisionItemResponse
    {
        public Guid? Id { get; set; }
        public Guid QualityCriterionId { get; set; }
        public string? CriterionQuestion { get; set; }
        public QualityAssessmentDecisionValue? Value { get; set; }
        public string? Comment { get; set; }
        public string? PdfHighlightCoordinates { get; set; }
    }

    public class CreateQualityAssessmentResolutionRequest
    {
        public Guid QualityAssessmentProcessId { get; set; }
        public Guid PaperId { get; set; }
        public QualityAssessmentResolutionDecision FinalDecision { get; set; }
        public decimal? FinalScore { get; set; }
        public string? ResolutionNotes { get; set; }
    }

    public class UpdateQualityAssessmentResolutionRequest
    {
        public Guid? Id { get; set; }
        public QualityAssessmentResolutionDecision FinalDecision { get; set; }
        public decimal? FinalScore { get; set; }
        public string? ResolutionNotes { get; set; }
    }

    public class CreateQualityAssessmentAssignmentRequest
    {
        public Guid QualityAssessmentProcessId { get; set; }
        public List<Guid> UserIds { get; set; } = new();
        public List<Guid> PaperIds { get; set; } = new();
    }

    public class CreateQualityAssessmentDecisionRequest
    {
        public Guid QualityAssessmentProcessId { get; set; }
        public Guid PaperId { get; set; }
        public decimal? Score { get; set; }
        public string? Notes { get; set; }
        public List<CreateQualityAssessmentDecisionItemRequest> DecisionItems { get; set; } = new();
    }

    public class CreateQualityAssessmentDecisionItemRequest
    {
        public Guid QualityCriterionId { get; set; }
        public QualityAssessmentDecisionValue? Value { get; set; }
        public string? Comment { get; set; }
        public string? PdfHighlightCoordinates { get; set; }
    }

    public class UpdateQualityAssessmentDecisionRequest
    {
        public Guid? Id { get; set; }
        public decimal? Score { get; set; }
        public string? Notes { get; set; }
        public List<UpdateQualityAssessmentDecisionItemRequest> DecisionItems { get; set; } = new();
    }

    public class UpdateQualityAssessmentDecisionItemRequest
    {
        public Guid? Id { get; set; }
        public Guid? QualityCriterionId { get; set; }
        public QualityAssessmentDecisionValue? Value { get; set; }
        public string? Comment { get; set; }
        public string? PdfHighlightCoordinates { get; set; }
    }

    public class AutomateQualityAssessmentRequest
    {
        public Guid QualityAssessmentProcessId { get; set; }
        public Guid PaperId { get; set; }
    }

    public class AutoResolveQualityAssessmentRequest
    {
        public Guid QualityAssessmentProcessId { get; set; }
        public double? Score { get; set; }
        public double? Percentage { get; set; }
    }

    public class QAPaperResponse
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

    /// <summary>
    /// For reviewer
    /// </summary>
    public class QAMemberDashboardPaperResponse : IncludedPaperResponse
    {
        public Guid Id { get; set; }
        /// <summary>
        /// Tính theo số lượng criterion đã hoàn thành trên tổng số criterion được yêu cầu đánh giá
        /// </summary>
        public double CompletionPercentage { get; set; }
        public QualityAssessmentResolutionResponse? Resolution { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<QualityAssessmentDecisionResponse> Decisions { get; set; } = new();
    }

    /// <summary>
    ///  For leader
    /// </summary>
    public class QALeaderDashboardPaperResponse : IncludedPaperResponse
    {
        public List<QualityAssessmentReviewerResponse> Reviewers { get; set; } = new();
        public List<QualityAssessmentDecisionResponse> Decisions { get; set; } = new();
        public QualityAssessmentResolutionResponse? Resolution { get; set; }
        /// <summary>
        /// Tính theo số lượng reviewer đã hoàn thành trên tổng số reviewer được phân công
        /// </summary>
        public double CompletionPercentage { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class QAMemberDashboardResponse
    {
        public PaginatedResponse<QAMemberDashboardPaperResponse> Papers { get; set; } = new();
        public double CompletionPercentage { get; set; }
        public int TotalPapers { get; set; }
        public int CompletedPapers { get; set; }
        public int InProgressPapers { get; set; }
        public int NotStartedPapers { get; set; }
    }

    public class QALeaderDashboardResponse
    {
        public PaginatedResponse<QALeaderDashboardPaperResponse> Papers { get; set; } = new();
        public List<QAReviewerProgressResponse> ReviewerProgresses { get; set; } = new();
        public int TotalPapers { get; set; }
        public int CompletedPapers { get; set; }
        public int InProgressPapers { get; set; }
        public int NotStartedPapers { get; set; }
    }

    public class QAReviewerProgressResponse
    {
        public Guid ReviewerId { get; set; }
        public string? ReviewerName { get; set; }
        public double CompletionPercentage { get; set; }
        public int CompletedPapers { get; set; }
        public int InProgressPapers { get; set; }
        public int NotStartedPapers { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public int TotalExpectedDecisions { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public int ActualDecisions { get; set; }
    }

    public class QualityAssessmentReviewerResponse
    {
        public Guid Id { get; set; }
        public string? Username { get; set; }
        public string? FullName { get; set; }
    }

    public class QualityAssessmentResolutionResponse
    {
        public Guid? Id { get; set; }
        public Guid QualityAssessmentProcessId { get; set; }
        public Guid PaperId { get; set; }
        public QualityAssessmentResolutionDecision FinalDecision { get; set; }
        public decimal? FinalScore { get; set; }
        public string? ResolutionNotes { get; set; }
        public Guid ResolvedBy { get; set; }
        public string? ResolvedByName { get; set; }
        public DateTimeOffset ResolvedAt { get; set; }
    }

    public class QualityAssessmentDecisionItemAIResponse
    {
        public Guid QualityCriterionId { get; set; }
        public QualityAssessmentDecisionValue? Value { get; set; }
        public string? Comment { get; set; }
        public string? PdfHighlightCoordinates { get; set; }
    }
}
