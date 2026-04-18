using System.ComponentModel.DataAnnotations;
using SRSS.IAM.Repositories.Entities.Enums;

namespace SRSS.IAM.Services.DTOs.Synthesis
{
	public class DataSynthesisStrategyDto
	{
		public Guid? SynthesisStrategyId { get; set; }

		[Required(ErrorMessage = "ProtocolId là bắt buộc")]
		public Guid ProtocolId { get; set; }

		[Required(ErrorMessage = "SynthesisType là bắt buộc")]
		public SynthesisType SynthesisType { get; set; }

		[StringLength(2000, ErrorMessage = "Description không được vượt quá 2000 ký tự")]
		public string? Description { get; set; }

		public List<Guid> TargetResearchQuestionIds { get; set; } = new List<Guid>();

		public string? DataGroupingPlan { get; set; }

		public string? SensitivityAnalysisPlan { get; set; }
	}




}