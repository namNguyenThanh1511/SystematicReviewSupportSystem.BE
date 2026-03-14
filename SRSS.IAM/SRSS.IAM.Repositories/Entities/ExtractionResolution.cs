using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
	/// <summary>
	/// Lưu final answer sau khi resolve conflict
	/// </summary>
	public class ExtractionResolution : BaseEntity<Guid>
	{
		public Guid StudyAssignmentId { get; set; }
		public Guid FieldId { get; set; }

		/// <summary>
		/// Loại resolution: UseSubmissionA, UseSubmissionB, Manual
		/// </summary>
		public ConsensusResolutionTypeEnum ResolutionType { get; set; }

		/// <summary>
		/// Người resolve
		/// </summary>
		public Guid ResolverUserId { get; set; }
		public string ResolverName { get; set; } = string.Empty;

		/// <summary>
		/// Ghi chú khi resolve
		/// </summary>
		public string? ResolutionNote { get; set; }

		// Final answer value
		public ExtractionAnswerValueKindEnum ValueKind { get; set; }
		public string? TextValue { get; set; }
		public decimal? NumberValue { get; set; }
		public bool? BooleanValue { get; set; }
		public Guid? OptionId { get; set; }
		public string? OptionIds { get; set; }

		/// <summary>
		/// Thời điểm resolve
		/// </summary>
		public DateTimeOffset ResolvedAt { get; set; } = DateTimeOffset.UtcNow;

		// Navigation properties
		public ExtractionStudyAssignment? StudyAssignment { get; set; }
		public ExtractionField? Field { get; set; }
	}
}