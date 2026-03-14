using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
	/// <summary>
	/// Lưu draft (auto-save) của reviewer cho 1 paper
	/// </summary>
	public class ExtractionDraft : BaseEntity<Guid>
	{
		public Guid StudyAssignmentId { get; set; }
		public Guid TemplateId { get; set; }

		/// <summary>
		/// Reviewer tạo draft này
		/// </summary>
		public Guid ReviewerUserId { get; set; }
		public string ReviewerName { get; set; } = string.Empty;

		/// <summary>
		/// Version của draft (increment mỗi khi save)
		/// </summary>
		public int DraftVersion { get; set; } = 1;

		/// <summary>
		/// Có phải auto-save không
		/// </summary>
		public bool IsAutosave { get; set; } = true;

		// Navigation properties
		public ExtractionStudyAssignment? StudyAssignment { get; set; }
		public ExtractionTemplate? Template { get; set; }
		public ICollection<ExtractionAnswer> Answers { get; set; } = new List<ExtractionAnswer>();
	}
}