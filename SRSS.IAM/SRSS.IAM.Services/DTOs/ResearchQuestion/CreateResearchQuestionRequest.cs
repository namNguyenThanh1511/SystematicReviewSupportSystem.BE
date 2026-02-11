using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Services.DTOs.ResearchQuestion
{
	public class CreateResearchQuestionRequest
	{
		[Required(ErrorMessage = "ProjectId là bắt buộc")]
		public Guid ProjectId { get; set; }

		[Required(ErrorMessage = "QuestionTypeId là bắt buộc")]
		public Guid QuestionTypeId { get; set; }

		[Required(ErrorMessage = "Question text là bắt buộc")]
		[StringLength(2000, ErrorMessage = "Question text không được vượt quá 2000 ký tự")]
		public string QuestionText { get; set; } = string.Empty;

		public string? Rationale { get; set; }

		public List<CreatePicocElementRequest> PicocElements { get; set; } = new();
	}

	public class CreatePicocElementRequest
	{
		[Required(ErrorMessage = "Element type là bắt buộc")]
		[RegularExpression("^(Population|Intervention|Comparison|Outcome|Context)$",
			ErrorMessage = "Element type phải là: Population, Intervention, Comparison, Outcome, hoặc Context")]
		public string ElementType { get; set; } = string.Empty;

		[Required(ErrorMessage = "Description là bắt buộc")]
		public string Description { get; set; } = string.Empty;

		// Specific fields for each type
		public PopulationDetailDto? PopulationDetail { get; set; }
		public InterventionDetailDto? InterventionDetail { get; set; }
		public ComparisonDetailDto? ComparisonDetail { get; set; }
		public OutcomeDetailDto? OutcomeDetail { get; set; }
		public ContextDetailDto? ContextDetail { get; set; }
	}
	public class PopulationDetailDto
	{
		public string Description { get; set; } = string.Empty;
	}

	public class InterventionDetailDto
	{
		public string Description { get; set; } = string.Empty;
	}

	public class ComparisonDetailDto
	{
		public string Description { get; set; } = string.Empty;
	}

	public class OutcomeDetailDto
	{
		public string? Metric { get; set; }
		public string Description { get; set; } = string.Empty;
	}

	public class ContextDetailDto
	{
		public string? Environment { get; set; }
		public string Description { get; set; } = string.Empty;
	}
}

