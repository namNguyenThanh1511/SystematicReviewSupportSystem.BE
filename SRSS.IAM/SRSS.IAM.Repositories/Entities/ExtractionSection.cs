using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
	/// <summary>
	/// Section trong Extraction Template (e.g., Identification, Methods, Interventions).
	/// Mỗi section có thể là FlatForm hoặc MatrixGrid.
	/// </summary>
	public class ExtractionSection : BaseEntity<Guid>
	{
		public Guid TemplateId { get; set; }

		public string Name { get; set; } = string.Empty;

		public string? Description { get; set; }

		public SectionType SectionType { get; set; }

		public int OrderIndex { get; set; }

		// Navigation properties
		public ExtractionTemplate? Template { get; set; }
		public ICollection<ExtractionField> Fields { get; set; } = new List<ExtractionField>();
		public ICollection<ExtractionMatrixColumn> MatrixColumns { get; set; } = new List<ExtractionMatrixColumn>();
	}
}
