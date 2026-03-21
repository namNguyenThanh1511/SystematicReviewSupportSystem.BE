using Shared.Entities.BaseEntity;
using SRSS.IAM.Repositories.Entities.Enums;

namespace SRSS.IAM.Repositories.Entities
{
    public class QualityAssessmentDecisionItem : BaseEntity<Guid>
    {
        public Guid QualityAssessmentDecisionId { get; set; }
        public Guid QualityCriterionId { get; set; }
        public QualityAssessmentDecisionValue? Value { get; set; } // The answer/score
        public string? Comment { get; set; }

        // Navigation Properties
        public QualityAssessmentDecision QualityAssessmentDecision { get; set; } = null!;
        public QualityCriterion QualityCriterion { get; set; } = null!;
    }
}
