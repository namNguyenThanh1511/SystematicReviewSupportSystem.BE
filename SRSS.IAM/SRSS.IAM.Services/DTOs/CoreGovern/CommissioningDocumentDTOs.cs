using System.ComponentModel.DataAnnotations;

namespace SRSS.IAM.Services.DTOs.CoreGovern
{
	public class CreateCommissioningDocumentRequest
	{
		[Required(ErrorMessage = "ProjectId là bắt buộc")]
		public Guid ProjectId { get; set; }

		public string? Sponsor { get; set; }

		public string? Scope { get; set; }

		[Range(0, double.MaxValue, ErrorMessage = "Budget phải là số dương")]
		public decimal? Budget { get; set; }

		public string? DocumentUrl { get; set; }
	}

	public class UpdateCommissioningDocumentRequest
	{
		[Required]
		public Guid Id { get; set; }

		public string? Sponsor { get; set; }

		public string? Scope { get; set; }

		[Range(0, double.MaxValue, ErrorMessage = "Budget phải là số dương")]
		public decimal? Budget { get; set; }

		public string? DocumentUrl { get; set; }
	}

	public class CommissioningDocumentResponse
	{
		public Guid Id { get; set; }
		public Guid ProjectId { get; set; }
		public string? Sponsor { get; set; }
		public string? Scope { get; set; }
		public decimal? Budget { get; set; }
		public string? DocumentUrl { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset ModifiedAt { get; set; }
	}
}
