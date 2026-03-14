using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
	public class QualityAssessmentStrategy : BaseEntity<Guid>
	{
		public Guid ProtocolId { get; set; }
		public string? Description { get; set; }

		// Navigation properties
		public ReviewProtocol Protocol { get; set; } = null!;
		public ICollection<QualityChecklist> Checklists { get; set; } = new List<QualityChecklist>();
	}
}