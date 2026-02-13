using System.ComponentModel.DataAnnotations;

namespace SRSS.IAM.Services.DTOs.Synthesis
{
	public class DataSynthesisStrategyDto
	{
		public Guid? SynthesisStrategyId { get; set; }

		[Required(ErrorMessage = "ProtocolId là bắt buộc")]
		public Guid ProtocolId { get; set; }

		[Required(ErrorMessage = "SynthesisType là bắt buộc")]
		[StringLength(200, ErrorMessage = "SynthesisType không được vượt quá 200 ký tự")]
		public string SynthesisType { get; set; } = string.Empty;

		[StringLength(2000, ErrorMessage = "Description không được vượt quá 2000 ký tự")]
		public string? Description { get; set; }
	}

	public class DisseminationStrategyDto
	{
		public Guid? DisseminationId { get; set; }

		[Required(ErrorMessage = "ProtocolId là bắt buộc")]
		public Guid ProtocolId { get; set; }

		[Required(ErrorMessage = "Channel là bắt buộc")]
		[StringLength(200, ErrorMessage = "Channel không được vượt quá 200 ký tự")]
		public string Channel { get; set; } = string.Empty;

		[StringLength(2000, ErrorMessage = "Description không được vượt quá 2000 ký tự")]
		public string? Description { get; set; }
	}

	public class ProjectTimetableDto
	{
		public Guid? TimetableId { get; set; }

		[Required(ErrorMessage = "ProtocolId là bắt buộc")]
		public Guid ProtocolId { get; set; }

		[Required(ErrorMessage = "Milestone là bắt buộc")]
		[StringLength(500, ErrorMessage = "Milestone không được vượt quá 500 ký tự")]
		public string Milestone { get; set; } = string.Empty;

		public DateOnly? PlannedDate { get; set; }
	}
}