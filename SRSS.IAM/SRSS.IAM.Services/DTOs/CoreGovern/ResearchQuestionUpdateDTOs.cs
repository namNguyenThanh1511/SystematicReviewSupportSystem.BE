using System.ComponentModel.DataAnnotations;

namespace SRSS.IAM.Services.DTOs.CoreGovern
{
	public class UpdateResearchQuestionRequest
	{
		[Required]
		public Guid Id { get; set; }

		[Required(ErrorMessage = "QuestionTypeId là bắt buộc")]
		public Guid QuestionTypeId { get; set; }

		[Required(ErrorMessage = "QuestionText là bắt buộc")]
		[StringLength(2000, ErrorMessage = "QuestionText không được vượt quá 2000 ký tự")]
		public string QuestionText { get; set; } = string.Empty;

		public string? Rationale { get; set; }
	}

	public class UpdatePicocElementRequest
	{
		[Required]
		public Guid Id { get; set; }

		[Required(ErrorMessage = "Description là bắt buộc")]
		public string Description { get; set; } = string.Empty;

		// Specific fields per type (only the relevant one will be used)
		public UpdatePopulationDetailDto? PopulationDetail { get; set; }
		public UpdateInterventionDetailDto? InterventionDetail { get; set; }
		public UpdateComparisonDetailDto? ComparisonDetail { get; set; }
		public UpdateOutcomeDetailDto? OutcomeDetail { get; set; }
		public UpdateContextDetailDto? ContextDetail { get; set; }
	}

	public class UpdatePopulationDetailDto
	{
		[Required(ErrorMessage = "Description là bắt buộc")]
		public string Description { get; set; } = string.Empty;
	}

	public class UpdateInterventionDetailDto
	{
		[Required(ErrorMessage = "Description là bắt buộc")]
		public string Description { get; set; } = string.Empty;
	}

	public class UpdateComparisonDetailDto
	{
		[Required(ErrorMessage = "Description là bắt buộc")]
		public string Description { get; set; } = string.Empty;
	}

	public class UpdateOutcomeDetailDto
	{
		public string? Metric { get; set; }

		[Required(ErrorMessage = "Description là bắt buộc")]
		public string Description { get; set; } = string.Empty;
	}

	public class UpdateContextDetailDto
	{
		public string? Environment { get; set; }

		[Required(ErrorMessage = "Description là bắt buộc")]
		public string Description { get; set; } = string.Empty;
	}

	public class AddPicocElementRequest
	{
		[Required]
		public Guid ResearchQuestionId { get; set; }

		[Required(ErrorMessage = "Element type là bắt buộc")]
		[RegularExpression("^(Population|Intervention|Comparison|Outcome|Context)$",
			ErrorMessage = "ElementType phải là: Population, Intervention, Comparison, Outcome, hoặc Context")]
		public string ElementType { get; set; } = string.Empty;

		[Required(ErrorMessage = "Description là bắt buộc")]
		public string Description { get; set; } = string.Empty;

		public UpdatePopulationDetailDto? PopulationDetail { get; set; }
		public UpdateInterventionDetailDto? InterventionDetail { get; set; }
		public UpdateComparisonDetailDto? ComparisonDetail { get; set; }
		public UpdateOutcomeDetailDto? OutcomeDetail { get; set; }
		public UpdateContextDetailDto? ContextDetail { get; set; }
	}
}
