using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
	public class QualityCriterion : BaseEntity<Guid>
	{
		public Guid ChecklistId { get; set; }
		public string Question { get; set; } = string.Empty;
		public decimal Weight { get; set; }

		// Navigation properties
		public QualityChecklist Checklist { get; set; } = null!;
	}
}