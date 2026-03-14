using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
	/// <summary>
	/// Ghi nhận conflict khi 2 reviewer submit khác nhau cho cùng 1 field
	/// </summary>
	public class ExtractionConflict : BaseEntity<Guid>
	{
		public Guid StudyAssignmentId { get; set; }
		public Guid FieldId { get; set; }

		/// <summary>
		/// 2 submission đang conflict
		/// </summary>
		public Guid SubmissionAId { get; set; }
		public Guid SubmissionBId { get; set; }

		/// <summary>
		/// Được resolve hay chưa
		/// </summary>
		public bool IsResolved { get; set; } = false;

		/// <summary>
		/// ID của resolution (nếu đã resolve)
		/// </summary>
		public Guid? ResolutionId { get; set; }

		// Navigation properties
		public ExtractionStudyAssignment? StudyAssignment { get; set; }
		public ExtractionField? Field { get; set; }
		public ExtractionSubmission? SubmissionA { get; set; }
		public ExtractionSubmission? SubmissionB { get; set; }
		public ExtractionResolution? Resolution { get; set; }
	}
}