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

		// Navigation properties
		public Paper Paper { get; set; } = null!;
		public ExtractionField Field { get; set; } = null!;
		public User Reviewer { get; set; } = null!;
		public FieldOption? Option { get; set; }
	}
}