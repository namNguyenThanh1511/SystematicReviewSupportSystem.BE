using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
	/// <summary>
	/// Cột trong MatrixGrid section (trục X của ma trận).
	/// Ví dụ: "Proposed Algorithm", "Baseline Method".
	/// </summary>
	public class ExtractionMatrixColumn : BaseEntity<Guid>
	{
		public Guid SectionId { get; set; }

		public string Name { get; set; } = string.Empty;

		public string? Description { get; set; }

		public int OrderIndex { get; set; }

		// Navigation properties
		public ExtractionSection? Section { get; set; }
	}
}
