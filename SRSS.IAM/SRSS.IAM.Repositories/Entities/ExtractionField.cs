using Shared.Entities.BaseEntity;
using System.ComponentModel.DataAnnotations;

namespace SRSS.IAM.Repositories.Entities
{
	/// <summary>
	/// Trường dữ liệu / Node (Hỗ trợ cấu trúc cây - Self-referencing)
	/// </summary>
	public class ExtractionField : BaseEntity<Guid>
	{
		public Guid TemplateId { get; set; }

		/// <summary>
		/// Self-referencing: Null = Root Field, có giá trị = Sub-field
		/// </summary>
		public Guid? ParentFieldId { get; set; }

		[MaxLength(500)]
		public string Name { get; set; } = string.Empty;

		[MaxLength(2000)]
		public string? Instruction { get; set; }

		/// <summary>
		/// Enum: Text, Integer, Decimal, Boolean, SingleSelect, MultiSelect
		/// </summary>
		[MaxLength(50)]
		public string FieldType { get; set; } = string.Empty;

		public bool IsRequired { get; set; }

		public int OrderIndex { get; set; }

		// Navigation properties
		public ExtractionTemplate Template { get; set; } = null!;

		/// <summary>
		/// Self-reference: Parent field
		/// </summary>
		public ExtractionField? ParentField { get; set; }

		/// <summary>
		/// Self-reference: Child fields (Sub-items)
		/// </summary>
		public ICollection<ExtractionField> SubFields { get; set; } = new List<ExtractionField>();

		/// <summary>
		/// Options for SingleSelect/MultiSelect fields
		/// </summary>
		public ICollection<FieldOption> Options { get; set; } = new List<FieldOption>();
	}
}