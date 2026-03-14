using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
	/// <summary>
	/// Lưu context và cấu hình extraction cho 1 ReviewProcess
	/// </summary>
	public class ExtractionContext : BaseEntity<Guid>
	{
		public Guid ReviewProcessId { get; set; }
		public Guid ProtocolId { get; set; }

		/// <summary>
		/// Chế độ extraction: SingleExtraction (0) hoặc DoubleExtraction (1)
		/// </summary>
		public ExtractionModeEnum Mode { get; set; } = ExtractionModeEnum.SingleExtraction;

		/// <summary>
		/// Template mặc định (optional)
		/// </summary>
		public Guid? DefaultTemplateId { get; set; }

		/// <summary>
		/// Double blind mode cho DoubleExtraction
		/// </summary>
		public bool DoubleBlind { get; set; } = true;

		/// <summary>
		/// Khoảng thời gian tự động lưu draft (giây)
		/// </summary>
		public int AutoSaveIntervalSeconds { get; set; } = 30;

		/// <summary>
		/// Thời điểm phase hoàn thành
		/// </summary>
		public DateTimeOffset? CompletedAt { get; set; }

		/// <summary>
		/// Người hoàn thành phase
		/// </summary>
		public Guid? CompletedByUserId { get; set; }

		public bool IsCompleted { get; set; } = false;

		// Navigation properties
		public ReviewProcess? ReviewProcess { get; set; }
		public ExtractionTemplate? DefaultTemplate { get; set; }
		public ICollection<ExtractionStudyAssignment> StudyAssignments { get; set; } = new List<ExtractionStudyAssignment>();
	}
}