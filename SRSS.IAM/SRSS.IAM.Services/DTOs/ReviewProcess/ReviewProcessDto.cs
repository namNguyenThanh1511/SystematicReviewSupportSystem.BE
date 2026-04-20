using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Services.DTOs.DataExtraction;
using SRSS.IAM.Services.DTOs.Identification;
using SRSS.IAM.Services.DTOs.StudySelection;
using SRSS.IAM.Services.DTOs.QualityAssessment;
using SRSS.IAM.Repositories.Entities.Enums;

namespace SRSS.IAM.Services.DTOs.ReviewProcess
{
    public class CreateReviewProcessRequest
    {
        public string? Name { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateReviewProcessRequest
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Notes { get; set; }
    }

    public class ReviewProcessResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid ProjectId { get; set; }
        public ProcessStatus Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public ProcessPhase CurrentPhase { get; set; }
        public string CurrentPhaseText { get; set; } = string.Empty;
        public DateTimeOffset? StartedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
        public string? Notes { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset ModifiedAt { get; set; }

        public IdentificationProcessResponse? IdentificationProcess { get; set; }
        public StudySelectionProcessResponse? StudySelectionProcess { get; set; }
        public QualityAssessmentProcessResponse? QualityAssessmentProcess { get; set; }
        public DataExtractionProcessResponse? DataExtractionProcess { get; set; }
        public SynthesisProcessResponse? SynthesisProcess { get; set; }
    }

    public class SynthesisProcessResponse
    {
        public Guid Id { get; set; }
        public Guid ReviewProcessId { get; set; }
        public SynthesisProcessStatus Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public DateTimeOffset? StartedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset ModifiedAt { get; set; }
    }
}