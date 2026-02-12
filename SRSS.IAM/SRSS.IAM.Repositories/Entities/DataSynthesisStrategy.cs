using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
	public class DataSynthesisStrategy : BaseEntity<Guid>
	{
		public Guid ProtocolId { get; set; }
		public string SynthesisType { get; set; } = string.Empty;
		public string? Description { get; set; }

		// Navigation properties
		public ReviewProtocol Protocol { get; set; } = null!;
	}
}