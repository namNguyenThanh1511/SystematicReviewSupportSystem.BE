namespace SRSS.IAM.Services.DTOs.DataExtraction
{
	public class ExtractionDashboardTaskDto
	{
		public Guid TaskId { get; set; }
		public Guid PaperId { get; set; }
		public string Title { get; set; } = string.Empty;
		public string? Authors { get; set; }
		public int? PublicationYear { get; set; }
		public Guid? Reviewer1Id { get; set; }
		public Guid? Reviewer2Id { get; set; }
		public string Status { get; set; } = string.Empty;
		public string? PdfUrl { get; set; }
	}
}
