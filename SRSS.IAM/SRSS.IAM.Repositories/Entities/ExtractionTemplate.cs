using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
	public class ExtractionTemplate : BaseEntity<Guid>
	{
		public Guid DataExtractionProcessId { get; set; }

		public string Name { get; set; } = string.Empty;

		public string? Description { get; set; }

		// Navigation properties
		public DataExtractionProcess DataExtractionProcess { get; set; } = null!;
		public ICollection<ExtractionSection> Sections { get; set; } = new List<ExtractionSection>();
	}
}