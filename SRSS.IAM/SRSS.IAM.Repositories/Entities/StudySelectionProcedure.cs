using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
	public class StudySelectionProcedure : BaseEntity<Guid>
	{
		public Guid ProtocolId { get; set; }
		public string Steps { get; set; } = string.Empty;

		// Navigation properties
		public ReviewProtocol Protocol { get; set; } = null!;
	}
}