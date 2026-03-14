using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
	/// <summary>
	/// Câu trả lời cho 1 field (có thể từ draft hoặc submission)
	/// </summary>
	public class ExtractionAnswer : BaseEntity<Guid>
	{
		public Guid FieldId { get; set; }

		/// <summary>
		/// Có thể từ draft hoặc submission
		/// </summary>
		public Guid? DraftId { get; set; }
		public Guid? SubmissionId { get; set; }

		/// <summary>
		/// Loại giá trị (Text, Number, Boolean, SingleSelect, MultiSelect)
		/// </summary>
		public ExtractionAnswerValueKindEnum ValueKind { get; set; }

		// Flexible value storage
		public string? TextValue { get; set; }
		public decimal? NumberValue { get; set; }
		public bool? BooleanValue { get; set; }
		public Guid? OptionId { get; set; } // Cho SingleSelect
		public string? OptionIds { get; set; } // JSON array cho MultiSelect

		// Evidence tracking
		public string? EvidenceQuote { get; set; }
		public int? EvidencePageNumber { get; set; }
		public string? EvidenceSource { get; set; }

		// Navigation properties
		public ExtractionField? Field { get; set; }
		public ExtractionDraft? Draft { get; set; }
		public ExtractionSubmission? Submission { get; set; }
	}
}