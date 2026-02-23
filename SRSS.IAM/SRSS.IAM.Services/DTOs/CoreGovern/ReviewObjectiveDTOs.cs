using System.ComponentModel.DataAnnotations;

namespace SRSS.IAM.Services.DTOs.CoreGovern
{
	public class CreateReviewObjectiveRequest
	{
		[Required(ErrorMessage = "ProjectId là bắt buộc")]
		public Guid ProjectId { get; set; }

		[Required(ErrorMessage = "ObjectiveStatement là bắt buộc")]
		[StringLength(2000, ErrorMessage = "ObjectiveStatement không được vượt quá 2000 ký tự")]
		public string ObjectiveStatement { get; set; } = string.Empty;
	}

	public class UpdateReviewObjectiveRequest
	{
		[Required]
		public Guid Id { get; set; }

		[Required(ErrorMessage = "ObjectiveStatement là bắt buộc")]
		[StringLength(2000, ErrorMessage = "ObjectiveStatement không được vượt quá 2000 ký tự")]
		public string ObjectiveStatement { get; set; } = string.Empty;
	}

	public class ReviewObjectiveResponse
	{
		public Guid Id { get; set; }
		public Guid ProjectId { get; set; }
		public string ObjectiveStatement { get; set; } = string.Empty;
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset ModifiedAt { get; set; }
	}
}
