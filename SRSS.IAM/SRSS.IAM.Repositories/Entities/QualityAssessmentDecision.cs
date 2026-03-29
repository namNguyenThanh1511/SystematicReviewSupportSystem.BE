using Shared.Entities.BaseEntity;
using System.Collections.Generic;
using SRSS.IAM.Repositories.Entities.Enums;

namespace SRSS.IAM.Repositories.Entities
{
    public class QualityAssessmentDecision : BaseEntity<Guid>
    {
        public Guid QualityAssessmentProcessId { get; set; }
        public Guid ReviewerId { get; set; }
        public Guid PaperId { get; set; }
        public decimal? Score { get; set; }
        // Navigation Properties
        public User Reviewer { get; set; } = null!;
        public Paper Paper { get; set; } = null!;
        public QualityAssessmentProcess QualityAssessmentProcess { get; set; } = null!;
        public ICollection<QualityAssessmentDecisionItem> DecisionItems { get; set; } = new List<QualityAssessmentDecisionItem>();
    }
}
