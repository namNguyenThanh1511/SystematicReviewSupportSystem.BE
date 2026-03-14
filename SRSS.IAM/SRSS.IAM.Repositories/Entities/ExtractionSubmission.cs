using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
	/// <summary>
	/// Lưu submission (chính thức) của reviewer cho 1 paper
	/// </summary>
	public class ExtractionSubmission : BaseEntity<Guid>
	{
		public Guid StudyAssignmentId { get; set; }
		public Guid TemplateId { get; set; }

		/// <summary>
		/// Reviewer submit
		/// </summary>
		public Guid ReviewerUserId { get; set; }
		public string ReviewerName { get; set; } = string.Empty;

		/// <summary>
		/// Ghi chú khi submit
		/// </summary>
		public string? SubmissionNote { get; set; }

		/// <summary>
		/// Thời điểm submit
		/// </summary>
		public DateTimeOffset SubmittedAt { get; set; } = DateTimeOffset.UtcNow;

		// Navigation properties
		public ExtractionStudyAssignment? StudyAssignment { get; set; }
		public ExtractionTemplate? Template { get; set; }
		public ICollection<ExtractionAnswer> Answers { get; set; } = new List<ExtractionAnswer>();
	}
}