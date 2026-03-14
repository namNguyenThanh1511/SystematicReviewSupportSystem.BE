using System.ComponentModel.DataAnnotations;

namespace SRSS.IAM.Services.DTOs.SelectionCriteria
{
	public class StudySelectionCriteriaDto
	{
		public Guid? CriteriaId { get; set; }

		[Required(ErrorMessage = "ProtocolId là bắt buộc")]
		public Guid ProtocolId { get; set; }

		[StringLength(2000, ErrorMessage = "Description không được vượt quá 2000 ký tự")]
		public string? Description { get; set; }
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

	public class StudySelectionProcedureDto
	{
		public Guid? ProcedureId { get; set; }

		[Required(ErrorMessage = "ProtocolId là bắt buộc")]
		public Guid ProtocolId { get; set; }

		[Required(ErrorMessage = "Steps là bắt buộc")]
		public string Steps { get; set; } = string.Empty;
	}
}