using System.ComponentModel.DataAnnotations;

namespace SRSS.IAM.Services.DTOs.DataExtraction
{
	public class ExtractionTemplateDto
	{
		public Guid? TemplateId { get; set; }

		[Required(ErrorMessage = "ProtocolId là bắt buộc")]
		public Guid ProtocolId { get; set; }

		[Required(ErrorMessage = "Name là bắt buộc")]
		[StringLength(500, ErrorMessage = "Name không được vượt quá 500 ký tự")]
		public string Name { get; set; } = string.Empty;

		[StringLength(2000, ErrorMessage = "Description không được vượt quá 2000 ký tự")]
		public string? Description { get; set; }

		/// <summary>
		/// Cấu trúc cây các fields (recursive)
		/// </summary>
		public List<ExtractionFieldDto> Fields { get; set; } = new();
	}

	public class ExtractionFieldDto
	{
		public Guid? FieldId { get; set; }
		public Guid? TemplateId { get; set; }
		public Guid? ParentFieldId { get; set; }

		[Required(ErrorMessage = "Name là bắt buộc")]
		[StringLength(500, ErrorMessage = "Name không được vượt quá 500 ký tự")]
		public string Name { get; set; } = string.Empty;

		[StringLength(2000)]
		public string? Instruction { get; set; }

		[Required(ErrorMessage = "FieldType là bắt buộc")]
		public FieldTypeEnum FieldType { get; set; }

		public bool IsRequired { get; set; }
		public int OrderIndex { get; set; }

		/// <summary>
		/// Options cho SingleSelect/MultiSelect
		/// </summary>
		public List<FieldOptionDto> Options { get; set; } = new();

		/// <summary>
		/// Sub-fields (nested structure)
		/// </summary>
		public List<ExtractionFieldDto> SubFields { get; set; } = new();
	}

	public class FieldOptionDto
	{
		public Guid? OptionId { get; set; }
		public Guid? FieldId { get; set; }

		[Required(ErrorMessage = "Value là bắt buộc")]
		[StringLength(500)]
		public string Value { get; set; } = string.Empty;

		public int DisplayOrder { get; set; }
	}

	public enum FieldTypeEnum
	{
		Text,
		Integer,
		Decimal,
		Boolean,
		SingleSelect,
		MultiSelect
	}
}