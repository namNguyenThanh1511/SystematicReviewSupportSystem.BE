using Shared.Entities.BaseEntity;
using SRSS.IAM.Repositories.Entities.Enums;

namespace SRSS.IAM.Repositories.Entities
{
    public class QualityAssessmentDecision : BaseEntity<Guid>
    {
        public Guid ReviewerId { get; set; } // The reviewer who made this decision
        public Guid PaperId { get; set; }
        public Guid QualityCriterionId { get; set; }
        public QualityAssessmentDecisionValue? Value { get; set; } // The answer/score
        public string? Comment { get; set; }

        // Navigation Properties
        public User Reviewer { get; set; } = null!;
        public Paper Paper { get; set; } = null!;
        public QualityCriterion QualityCriterion { get; set; } = null!;
    }
}
