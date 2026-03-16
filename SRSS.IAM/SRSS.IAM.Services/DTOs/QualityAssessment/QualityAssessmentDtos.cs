using System.Text.Json.Serialization;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;
using SRSS.IAM.Services.DTOs.Paper;

namespace SRSS.IAM.Services.DTOs.QualityAssessment
{
    public class QualityAssessmentProcessResponse
    {
        public Guid Id { get; set; }
        public Guid ReviewProcessId { get; set; }
        public string? Notes { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public QualityAssessmentProcessStatus Status { get; set; }
        public DateTimeOffset? StartedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
        // Keep these simplified for basic response, allow expansion
    }

    public class CreateQualityAssessmentProcessDto
    {
        public Guid ReviewProcessId { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateQualityAssessmentProcessDto
    {
        public string? Notes { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public QualityAssessmentProcessStatus Status { get; set; }
    }

    public class QualityAssessmentAssignmentDto
    {
        public Guid? Id { get; set; }
        public Guid QualityAssessmentProcessId { get; set; }
        public Guid UserId { get; set; }
        public string? UserName { get; set; }
        public DateTimeOffset AssignedAt { get; set; }
        public List<PaperResponse>? Papers { get; set; }
        public List<QualityAssessmentDecisionDto>? Decisions { get; set; }
    }

    public class QualityAssessmentDecisionDto
    {
        public Guid? Id { get; set; }
        public Guid ReviewerId { get; set; }
        public string? ReviewerName { get; set; }
        public Guid PaperId { get; set; }
        public Guid QualityCriterionId { get; set; }
        public string? CriterionQuestion { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public QualityAssessmentDecisionValue? Value { get; set; }
        public string? Comment { get; set; }
    }

    public class CreateQualityAssessmentResolutionDto
    {
        public Guid QualityAssessmentProcessId { get; set; }
        public Guid PaperId { get; set; }
        public QualityAssessmentResolutionDecision FinalDecision { get; set; }
        public decimal? FinalScore { get; set; }
        public string? ResolutionNotes { get; set; }
        public Guid ResolvedBy { get; set; }
    }

    public class UpdateQualityAssessmentResolutionDto
    {
        public QualityAssessmentResolutionDecision FinalDecision { get; set; }
        public decimal? FinalScore { get; set; }
        public string? ResolutionNotes { get; set; }
    }

    public class QualityAssessmentResolutionResponse : QualityAssessmentResolutionDto
    {
    }

    public class CreateQualityAssessmentAssignmentDto
    {
        public Guid QualityAssessmentProcessId { get; set; }
        public List<Guid> UserIds { get; set; } = new();
        public List<Guid> PaperIds { get; set; } = new();
    }

    public class CreateQualityAssessmentDecisionDto
    {
        public Guid PaperId { get; set; }
        public Guid QualityCriterionId { get; set; }
        public QualityAssessmentDecisionValue? Value { get; set; }
        public string? Comment { get; set; }
    }

    public class CreateQualityAssessmentDecisionItemDto
    {
        public Guid QualityCriterionId { get; set; }
        public QualityAssessmentDecisionValue? Value { get; set; }
        public string? Comment { get; set; }
    }

    public class UpdateQualityAssessmentDecisionDto
    {
        public QualityAssessmentDecisionValue? Value { get; set; }
        public string? Comment { get; set; }
    }

    public class UpdateQualityAssessmentDecisionItemDto
    {
        public Guid QualityCriterionId { get; set; }
        public QualityAssessmentDecisionValue? Value { get; set; }
        public string? Comment { get; set; }
    }

    public class MyAssignedPaperDto : PaperResponse
    {
        public double CompletionPercentage { get; set; }
        public string? ResolutionDecision { get; set; }
    }

    public class QualityAssessmentSummaryDto
    {
        public Guid PaperId { get; set; }
        public string PaperTitle { get; set; } = string.Empty;
        public List<QualityAssessmentDecisionDto> Decisions { get; set; } = new();
        public QualityAssessmentResolutionDto? Resolution { get; set; }
        public double CompletionPercentage { get; set; }
    }

    public class QualityAssessmentResolutionDto
    {
        public Guid? Id { get; set; }
        public Guid QualityAssessmentProcessId { get; set; }
        public Guid PaperId { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public QualityAssessmentResolutionDecision FinalDecision { get; set; }
        public decimal? FinalScore { get; set; }
        public string? ResolutionNotes { get; set; }
        public Guid ResolvedBy { get; set; }
        public string? ResolvedByName { get; set; }
        public DateTimeOffset ResolvedAt { get; set; }
    }

    public class QualityAssessmentProcessDto
    {
        public Guid? Id { get; set; }
        public Guid ReviewProcessId { get; set; }
        public string? Notes { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public QualityAssessmentProcessStatus Status { get; set; }
        public DateTimeOffset? StartedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
        public List<QualityAssessmentAssignmentDto> Assignments { get; set; } = new();
        public List<QualityAssessmentResolutionDto> Resolutions { get; set; } = new();
    }
}
