using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
	public class DataExtractionStrategy : BaseEntity<Guid>
	{
		public Guid ProtocolId { get; set; }
		public string? Description { get; set; }

		// Navigation properties
		public ReviewProtocol Protocol { get; set; } = null!;
		public ICollection<DataExtractionForm> Forms { get; set; } = new List<DataExtractionForm>();
	}
}