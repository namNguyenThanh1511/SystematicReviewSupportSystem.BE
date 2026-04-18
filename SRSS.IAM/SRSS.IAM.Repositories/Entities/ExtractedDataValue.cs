using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
	/// <summary>
	/// Dữ liệu thực tế được trích xuất (Conducting Phase)
	/// </summary>
	public class ExtractedDataValue : BaseEntity<Guid>
	{
		/// <summary>
		/// ID của bài báo được trích xuất dữ liệu
		/// </summary>
		public Guid PaperId { get; set; }

		public Guid FieldId { get; set; }
		public Guid ReviewerId { get; set; }

		/// <summary>
		/// Nullable: Nếu chọn từ FieldOption
		/// </summary>
		public Guid? OptionId { get; set; }

		/// <summary>
		/// Lưu text nếu FieldType là Text
		/// </summary>
		public string? StringValue { get; set; }

		/// <summary>
		/// Lưu số nếu FieldType là Decimal/Integer
		/// </summary>
		public decimal? NumericValue { get; set; }

		public bool? BooleanValue { get; set; }

		public Guid? MatrixColumnId { get; set; }

		public int? MatrixRowIndex { get; set; }

		/// <summary>
		/// Indicates that the reviewer formally confirmed this data point was not reported in the primary study.
		/// When true, all value fields (StringValue, NumericValue, BooleanValue, OptionId) must be null.
		/// </summary>
		public bool IsNotReported { get; set; } = false;

		/// <summary>
		/// JSON-serialized array of bounding box coordinates (page, x, y, w, h) providing evidence from the PDF.
		/// </summary>
		public string? EvidenceCoordinates { get; set; }

		public bool IsConsensusFinal { get; set; } = false;

		// Navigation properties
		public Paper Paper { get; set; } = null!;
		public ExtractionField Field { get; set; } = null!;
		public User Reviewer { get; set; } = null!;
		public FieldOption? Option { get; set; }
		public ExtractionMatrixColumn? MatrixColumn { get; set; }
	}
}