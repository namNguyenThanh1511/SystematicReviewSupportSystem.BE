namespace SRSS.IAM.Services.DTOs.DataExtraction
{
	/// <summary>
	/// Giá trị của một câu trả lời (flexible theo FieldType)
	/// </summary>
	public class ExtractionAnswerValueDto
	{
		/// <summary>
		/// Loại giá trị: 0=Null, 1=Text, 2=Number, 3=Boolean, 4=SingleSelect, 5=MultiSelect
		/// </summary>
		public int Kind { get; set; }

		public string? TextValue { get; set; }
		public decimal? NumberValue { get; set; }
		public bool? BooleanValue { get; set; }
		public Guid? OptionId { get; set; } // SingleSelect
		public List<Guid>? OptionIds { get; set; } // MultiSelect
	}

	/// <summary>
	/// Evidence link (quote từ paper, page number, source)
	/// </summary>
	public class EvidenceLinkDto
	{
		public string? Quote { get; set; }
		public int? PageNumber { get; set; }
		public string? Source { get; set; }
	}

	/// <summary>
	/// Một câu trả lời cho một field
	/// </summary>
	public class ExtractionAnswerDto
	{
		public Guid FieldId { get; set; }
		public ExtractionAnswerValueDto Value { get; set; } = new();
		public EvidenceLinkDto? Evidence { get; set; }
	}
}