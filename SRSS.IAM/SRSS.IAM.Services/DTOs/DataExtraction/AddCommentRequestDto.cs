using System;
using System.ComponentModel.DataAnnotations;

namespace SRSS.IAM.Services.DTOs.DataExtraction
{
	public class AddCommentRequestDto
	{
		[Required]
		public Guid ThreadOwnerId { get; set; }

		[Required]
		public string Content { get; set; } = string.Empty;
		public Guid? MatrixColumnId { get; set; }
		public int? MatrixRowIndex { get; set; }
	}
}
