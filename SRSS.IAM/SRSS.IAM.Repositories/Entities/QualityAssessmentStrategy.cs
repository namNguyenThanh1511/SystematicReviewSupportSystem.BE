using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
	public class QualityAssessmentStrategy : BaseEntity<Guid>
	{
		public Guid QualityAssessmentProcessId { get; set; }
		public string Description { get; set; } = string.Empty;

		// Navigation properties
		public QualityAssessmentProcess QualityAssessmentProcess { get; set; } = null!;
		public ICollection<QualityChecklist> Checklists { get; set; } = new List<QualityChecklist>();
	}
}