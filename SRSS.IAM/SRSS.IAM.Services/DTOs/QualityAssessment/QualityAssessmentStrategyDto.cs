using System.ComponentModel.DataAnnotations;

namespace SRSS.IAM.Services.DTOs.QualityAssessment
{
	public class QualityAssessmentStrategyDto
	{
		public Guid? QaStrategyId { get; set; }

		[Required(ErrorMessage = "ProjectId là bắt buộc")]
		public Guid ProjectId { get; set; }

		[StringLength(2000, ErrorMessage = "Description không được vượt quá 2000 ký tự")]
		public string? Description { get; set; }

		// Full nested representation when returning strategy for a process
		public List<QualityAssessmentChecklistDto> Checklists { get; set; } = new();
	}

	public class QualityAssessmentChecklistDto
	{
		public Guid? ChecklistId { get; set; }

		[Required(ErrorMessage = "QaStrategyId là bắt buộc")]
		public Guid QaStrategyId { get; set; }

		[Required(ErrorMessage = "Name là bắt buộc")]
		[StringLength(500, ErrorMessage = "Name không được vượt quá 500 ký tự")]
		public string Name { get; set; } = string.Empty;

		// Nested criteria when returning full checklist
		public List<QualityAssessmentCriterionDto> Criteria { get; set; } = new();
	}

	public class QualityAssessmentCriterionDto
	{
		public Guid? CriterionId { get; set; }

		[Required(ErrorMessage = "ChecklistId là bắt buộc")]
		public Guid ChecklistId { get; set; }

		[Required(ErrorMessage = "Question là bắt buộc")]
		public string Question { get; set; } = string.Empty;

		[Range(0, 100, ErrorMessage = "Weight phải từ 0 đến 100")]
		public decimal Weight { get; set; }
	}
}