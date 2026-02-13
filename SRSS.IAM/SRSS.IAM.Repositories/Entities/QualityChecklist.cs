using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
	public class QualityChecklist : BaseEntity<Guid>
	{
		public Guid QaStrategyId { get; set; }
		public string Name { get; set; } = string.Empty;

		// Navigation properties
		public QualityAssessmentStrategy QaStrategy { get; set; } = null!;
		public ICollection<QualityCriterion> Criteria { get; set; } = new List<QualityCriterion>();
	}
}