using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
	public class DataExtractionForm : BaseEntity<Guid>
	{
		public Guid ExtractionStrategyId { get; set; }
		public string Name { get; set; } = string.Empty;

		// Navigation properties
		public DataExtractionStrategy ExtractionStrategy { get; set; } = null!;
		public ICollection<DataItemDefinition> DataItems { get; set; } = new List<DataItemDefinition>();
	}
}