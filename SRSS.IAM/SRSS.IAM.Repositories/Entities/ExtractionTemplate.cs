using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
	public class ExtractionTemplate : BaseEntity<Guid>
	{
		public Guid ProtocolId { get; set; }

		public string Name { get; set; } = string.Empty;

		public string? Description { get; set; }

		// Navigation properties
		public ReviewProtocol? Protocol { get; set; }
		public ICollection<ExtractionSection> Sections { get; set; } = new List<ExtractionSection>();
	}
}