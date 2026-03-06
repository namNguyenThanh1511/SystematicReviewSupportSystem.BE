using Shared.Entities.BaseEntity;
using System.ComponentModel.DataAnnotations;

namespace SRSS.IAM.Repositories.Entities
{
	/// <summary>
	/// Các lựa chọn cho Single/Multi Select fields
	/// </summary>
	public class FieldOption : BaseEntity<Guid>
	{
		public Guid FieldId { get; set; }

		[MaxLength(500)]
		public string Value { get; set; } = string.Empty;

		public int DisplayOrder { get; set; }

		// Navigation properties
		public ExtractionField Field { get; set; } = null!;
	}
}