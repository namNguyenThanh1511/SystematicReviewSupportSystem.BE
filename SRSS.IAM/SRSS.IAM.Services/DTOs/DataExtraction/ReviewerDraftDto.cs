namespace SRSS.IAM.Services.DTOs.DataExtraction
{
	/// <summary>
	/// Draft của reviewer (auto-save)
	/// </summary>
	public class ReviewerDraftDto
	{
		public Guid ReviewerUserId { get; set; }
		public string ReviewerName { get; set; } = string.Empty;
		public DateTimeOffset UpdatedAt { get; set; }
		public List<ExtractionAnswerDto> Answers { get; set; } = new();
	}

	/// <summary>
	/// Submission chính thức của reviewer
	/// </summary>
	public class ReviewerSubmissionDto
	{
		public Guid SubmissionId { get; set; }
		public Guid ReviewerUserId { get; set; }
		public string ReviewerName { get; set; } = string.Empty;
		public DateTimeOffset SubmittedAt { get; set; }
		public string? SubmissionNote { get; set; }
		public List<ExtractionAnswerDto> Answers { get; set; } = new();
	}
}