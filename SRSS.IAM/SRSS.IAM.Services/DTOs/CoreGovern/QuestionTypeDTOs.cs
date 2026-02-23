using System.ComponentModel.DataAnnotations;

namespace SRSS.IAM.Services.DTOs.CoreGovern
{
	public class CreateQuestionTypeRequest
	{
		[Required(ErrorMessage = "Name là bắt buộc")]
		[StringLength(200, ErrorMessage = "Name không được vượt quá 200 ký tự")]
		public string Name { get; set; } = string.Empty;

		public string? Description { get; set; }
	}

	public class UpdateQuestionTypeRequest
	{
		[Required]
		public Guid Id { get; set; }

		[Required(ErrorMessage = "Name là bắt buộc")]
		[StringLength(200, ErrorMessage = "Name không được vượt quá 200 ký tự")]
		public string Name { get; set; } = string.Empty;

		public string? Description { get; set; }
	}

	public class QuestionTypeResponse
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string? Description { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset ModifiedAt { get; set; }
	}
}
