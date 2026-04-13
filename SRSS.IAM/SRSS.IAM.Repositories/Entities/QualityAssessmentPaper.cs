using Microsoft.EntityFrameworkCore;
using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
    public class QualityAssessmentPaper : BaseEntity<Guid>
    {
        public Guid QualityAssessmentProcessId { get; set; }
        public Guid PaperId { get; set; }
        public QualityAssessmentProcess QualityAssessmentProcess { get; set; } = null!;
        public Paper Paper { get; set; } = null!;
        public ICollection<QualityAssessmentAssignment> QualityAssessmentAssignments { get; set; } = new List<QualityAssessmentAssignment>();
        public ICollection<QualityAssessmentDecision> QualityAssessmentDecisions { get; set; } = new List<QualityAssessmentDecision>();
        public QualityAssessmentResolution? QualityAssessmentResolution { get; set; }
    }
}
