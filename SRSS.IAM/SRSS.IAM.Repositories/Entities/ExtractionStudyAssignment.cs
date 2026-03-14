using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
	/// <summary>
	/// Gán 1 paper cho 1 extraction context với trạng thái
	/// </summary>
	public class ExtractionStudyAssignment : BaseEntity<Guid>
	{
		public Guid ExtractionContextId { get; set; }
		public Guid PaperId { get; set; }

		/// <summary>
		/// Reviewer phụ trách (có thể null nếu chưa gán)
		/// </summary>
		public Guid? AssigneeUserId { get; set; }
		public string? AssigneeName { get; set; }

		/// <summary>
		/// Trạng thái: ToDo, InProgress, AwaitingConsensus, Completed
		/// </summary>
		public ExtractionStudyStatusEnum Status { get; set; } = ExtractionStudyStatusEnum.ToDo;

		/// <summary>
		/// Số lượng conflict (recalculate khi có submission mới)
		/// </summary>
		public int ConflictCount { get; set; } = 0;

		public bool HasDraft { get; set; } = false;
		public bool HasSubmission { get; set; } = false;

		// Navigation properties
		public ExtractionContext? ExtractionContext { get; set; }
		public Paper? Paper { get; set; }
		public ICollection<ExtractionDraft> Drafts { get; set; } = new List<ExtractionDraft>();
		public ICollection<ExtractionSubmission> Submissions { get; set; } = new List<ExtractionSubmission>();
		public ICollection<ExtractionConflict> Conflicts { get; set; } = new List<ExtractionConflict>();
		public ICollection<ExtractionResolution> Resolutions { get; set; } = new List<ExtractionResolution>();
	}
}