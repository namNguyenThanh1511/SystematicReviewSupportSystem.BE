using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
    public class QualityAssessmentAssignment : BaseEntity<Guid>
    {
        public Guid QualityAssessmentProcessId { get; set; }
        public Guid UserId { get; set; } // The reviewer
        public DateTimeOffset AssignedAt { get; set; } = DateTimeOffset.UtcNow;

        // Navigation Properties
        public QualityAssessmentProcess QualityAssessmentProcess { get; set; } = null!;
        public ICollection<QualityAssessmentPaper> QualityAssessmentPapers { get; set; } = new List<QualityAssessmentPaper>(); // Papers assigned to this member for review
        public User User { get; set; } = null!;
    }
}