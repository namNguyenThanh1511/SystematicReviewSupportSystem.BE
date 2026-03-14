using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
	public class DataItemDefinition : BaseEntity<Guid>
	{
		public Guid FormId { get; set; }
		public string Name { get; set; } = string.Empty;
		public string DataType { get; set; } = string.Empty;
		public string? Description { get; set; }

		// Navigation properties
		public DataExtractionForm Form { get; set; } = null!;
	}
}