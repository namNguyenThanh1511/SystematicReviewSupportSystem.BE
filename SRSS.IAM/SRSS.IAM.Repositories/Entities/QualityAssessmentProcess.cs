using Shared.Entities.BaseEntity;
using SRSS.IAM.Repositories.Entities.Enums;

namespace SRSS.IAM.Repositories.Entities
{
    public class QualityAssessmentProcess : BaseEntity<Guid>
    {
        public Guid ReviewProcessId { get; set; }
        public string? Notes { get; set; }
        public QualityAssessmentProcessStatus Status { get; set; } = QualityAssessmentProcessStatus.NotStarted;
        public DateTimeOffset? StartedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }

        // Navigation Properties
        public ReviewProcess ReviewProcess { get; set; } = null!;
        public ICollection<QualityAssessmentDecision> QualityAssessmentDecisions { get; set; } = new List<QualityAssessmentDecision>();
        public ICollection<QualityAssessmentAssignment> QualityAssessmentAssignments { get; set; } = new List<QualityAssessmentAssignment>();
        public ICollection<QualityAssessmentResolution> QualityAssessmentResolutions { get; set; } = new List<QualityAssessmentResolution>();
    
        public void Start()
        {
            if (Status == QualityAssessmentProcessStatus.NotStarted)
            {
                Status = QualityAssessmentProcessStatus.InProgress;
                StartedAt = DateTimeOffset.UtcNow;
            }
        }

        public void Complete()
        {
            if (Status == QualityAssessmentProcessStatus.InProgress)
            {
                Status = QualityAssessmentProcessStatus.Completed;
                CompletedAt = DateTimeOffset.UtcNow;
            }
        }
    }
}
