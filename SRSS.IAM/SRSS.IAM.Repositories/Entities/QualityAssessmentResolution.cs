using Shared.Entities.BaseEntity;
using SRSS.IAM.Repositories.Entities.Enums;

namespace SRSS.IAM.Repositories.Entities
{
    /// <summary>
    /// Represents the final resolved quality assessment decision for a paper
    /// </summary>
    public class QualityAssessmentResolution : BaseEntity<Guid>
    {
        public Guid QualityAssessmentProcessId { get; set; }
        public Guid ResolvedBy { get; set; }
        public Guid PaperId { get; set; }
        public QualityAssessmentResolutionDecision FinalDecision { get; set; }
        public decimal? FinalScore { get; set; }
        public string? ResolutionNotes { get; set; }
        public DateTimeOffset ResolvedAt { get; set; }

        // Navigation Properties
        public QualityAssessmentProcess QualityAssessmentProcess { get; set; } = null!;
        public Paper Paper { get; set; } = null!;
        public User ResolvedByUser { get; set; } = null!;
    }
}
