using System.ComponentModel.DataAnnotations;

namespace SRSS.IAM.Services.DTOs.QualityAssessment
{
	public class QualityAssessmentStrategyDto
	{
		public Guid? QaStrategyId { get; set; }

		[Required(ErrorMessage = "ProtocolId là bắt buộc")]
		public Guid ProtocolId { get; set; }

		[StringLength(2000, ErrorMessage = "Description không được vượt quá 2000 ký tự")]
		public string? Description { get; set; }
	}

	public class QualityChecklistDto
	{
		public Guid? ChecklistId { get; set; }

		[Required(ErrorMessage = "QaStrategyId là bắt buộc")]
		public Guid QaStrategyId { get; set; }

		[Required(ErrorMessage = "Name là bắt buộc")]
		[StringLength(500, ErrorMessage = "Name không được vượt quá 500 ký tự")]
		public string Name { get; set; } = string.Empty;
	}

	public class QualityCriterionDto
	{
		public Guid? QualityCriterionId { get; set; }

		[Required(ErrorMessage = "ChecklistId là bắt buộc")]
		public Guid ChecklistId { get; set; }

		[Required(ErrorMessage = "Question là bắt buộc")]
		public string Question { get; set; } = string.Empty;

		[Range(0, 100, ErrorMessage = "Weight phải từ 0 đến 100")]
		public decimal Weight { get; set; }
	}
}