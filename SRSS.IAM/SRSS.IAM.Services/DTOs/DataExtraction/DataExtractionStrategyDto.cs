using System.ComponentModel.DataAnnotations;

namespace SRSS.IAM.Services.DTOs.DataExtraction
{
	public class DataExtractionStrategyDto
	{
		public Guid? ExtractionStrategyId { get; set; }

		[Required(ErrorMessage = "ProtocolId là bắt buộc")]
		public Guid ProtocolId { get; set; }

		[StringLength(2000, ErrorMessage = "Description không được vượt quá 2000 ký tự")]
		public string? Description { get; set; }
	}

	public class DataExtractionFormDto
	{
		public Guid? FormId { get; set; }

		[Required(ErrorMessage = "ExtractionStrategyId là bắt buộc")]
		public Guid ExtractionStrategyId { get; set; }

		[Required(ErrorMessage = "Name là bắt buộc")]
		[StringLength(500, ErrorMessage = "Name không được vượt quá 500 ký tự")]
		public string Name { get; set; } = string.Empty;
	}

	public class DataItemDefinitionDto
	{
		public Guid? DataItemId { get; set; }

		[Required(ErrorMessage = "FormId là bắt buộc")]
		public Guid FormId { get; set; }

		[Required(ErrorMessage = "Name là bắt buộc")]
		[StringLength(500, ErrorMessage = "Name không được vượt quá 500 ký tự")]
		public string Name { get; set; } = string.Empty;

		[Required(ErrorMessage = "DataType là bắt buộc")]
		[StringLength(100, ErrorMessage = "DataType không được vượt quá 100 ký tự")]
		public string DataType { get; set; } = string.Empty;

		[StringLength(2000, ErrorMessage = "Description không được vượt quá 2000 ký tự")]
		public string? Description { get; set; }
	}
}