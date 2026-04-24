using System.ComponentModel.DataAnnotations;

namespace SRSS.IAM.Services.DTOs.SelectionCriteria
{
	public class StudySelectionCriteriaDto
	{
		public Guid? CriteriaId { get; set; }

		[Required(ErrorMessage = "StudySelectionProcessId là bắt buộc")]
		public Guid StudySelectionProcessId { get; set; }

		[StringLength(2000, ErrorMessage = "Description không được vượt quá 2000 ký tự")]
		public string? Description { get; set; }

		public List<InclusionCriterionDto> InclusionCriteria { get; set; } = new();
		public List<ExclusionCriterionDto> ExclusionCriteria { get; set; } = new();
	}

	public class InclusionCriterionDto
	{
		public Guid? InclusionId { get; set; }

		[Required(ErrorMessage = "CriteriaId là bắt buộc")]
		public Guid CriteriaId { get; set; }

		[Required(ErrorMessage = "Rule là bắt buộc")]
		[StringLength(1000, ErrorMessage = "Rule không được vượt quá 1000 ký tự")]
		public string Rule { get; set; } = string.Empty;
	}

	public class ExclusionCriterionDto
	{
		public Guid? ExclusionId { get; set; }

		[Required(ErrorMessage = "CriteriaId là bắt buộc")]
		public Guid CriteriaId { get; set; }

		[Required(ErrorMessage = "Rule là bắt buộc")]
		[StringLength(1000, ErrorMessage = "Rule không được vượt quá 1000 ký tự")]
		public string Rule { get; set; } = string.Empty;
	}

	public class AICriteriaGenerationInput
	{
		public string? Population { get; set; }
		public string? Intervention { get; set; }
		public string? Comparator { get; set; }
		public string? Outcome { get; set; }
		public string? Context { get; set; }
		public List<string> ResearchQuestions { get; set; } = new();
	}

	public class AICriteriaResponse
	{
		public List<AICriteriaGroup> CriteriaGroups { get; set; } = new();
	}

	public class AICriteriaGroup
	{
		public string Description { get; set; } = string.Empty;
		public List<AIGeneratedCriterion> InclusionCriteria { get; set; } = new();
		public List<AIGeneratedCriterion> ExclusionCriteria { get; set; } = new();
	}

	public class AIGeneratedCriterion
	{
		public string Text { get; set; } = string.Empty;
		public List<AITraceabilitySource> Sources { get; set; } = new();
	}

	public class AITraceabilitySource
	{
		public string SourceType { get; set; } = string.Empty; // PICOC or RQ
		public string SourceId { get; set; } = string.Empty; // PICOC field name or full RQ text
	}

	public class SaveAICriteriaRequest
	{
		[Required]
		public Guid StudySelectionProcessId { get; set; }

		[Required]
		public string RawJson { get; set; } = string.Empty;

		public List<AICriteriaGroup> CriteriaGroups { get; set; } = new();
	}

	public class SaveAICriteriaRequestV2
	{
		[Required]
		public Guid StudySelectionProcessId { get; set; }

		[Required]
		public string RawJson { get; set; } = string.Empty;

		public List<AICriteriaGroupInput> CriteriaGroups { get; set; } = new();
	}

	public class AICriteriaGroupInput
	{
		public string Description { get; set; } = string.Empty;
		public List<string> InclusionCriteria { get; set; } = new();
		public List<string> ExclusionCriteria { get; set; } = new();
	}
}