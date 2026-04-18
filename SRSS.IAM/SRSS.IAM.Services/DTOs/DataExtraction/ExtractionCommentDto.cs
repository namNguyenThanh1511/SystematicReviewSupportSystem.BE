using System;

namespace SRSS.IAM.Services.DTOs.DataExtraction
{
	public class ExtractionCommentDto
	{
		public Guid Id { get; set; }
		public Guid FieldId { get; set; }
		public Guid ThreadOwnerId { get; set; }
		public Guid UserId { get; set; }
		public string UserName { get; set; } = string.Empty;
		public string Content { get; set; } = string.Empty;
		public DateTimeOffset CreatedAt { get; set; }
	}
}
