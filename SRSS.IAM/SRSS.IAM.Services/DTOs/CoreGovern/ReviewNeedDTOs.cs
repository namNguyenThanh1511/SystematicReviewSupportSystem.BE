using System.ComponentModel.DataAnnotations;

namespace SRSS.IAM.Services.DTOs.CoreGovern
{
	public class CreateReviewNeedRequest
	{
		[Required(ErrorMessage = "ProjectId là bắt buộc")]
		public Guid ProjectId { get; set; }

		[Required(ErrorMessage = "Description là bắt buộc")]
		[StringLength(2000, ErrorMessage = "Description không được vượt quá 2000 ký tự")]
		public string Description { get; set; } = string.Empty;

		public string? Justification { get; set; }

		public string? IdentifiedBy { get; set; }
	}

	public class UpdateReviewNeedRequest
	{
		[Required]
		public Guid Id { get; set; }

		[Required(ErrorMessage = "Description là bắt buộc")]
		[StringLength(2000, ErrorMessage = "Description không được vượt quá 2000 ký tự")]
		public string Description { get; set; } = string.Empty;

		public string? Justification { get; set; }

		public string? IdentifiedBy { get; set; }
	}

	public class ReviewNeedResponse
	{
		public Guid Id { get; set; }
		public Guid ProjectId { get; set; }
		public string Description { get; set; } = string.Empty;
		public string? Justification { get; set; }
		public string? IdentifiedBy { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset ModifiedAt { get; set; }
	}
}
