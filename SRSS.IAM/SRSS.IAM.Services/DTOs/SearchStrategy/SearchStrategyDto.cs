using System.ComponentModel.DataAnnotations;

namespace SRSS.IAM.Services.DTOs.SearchStrategy
{
	public class SearchStrategyDto
	{
		public Guid? StrategyId { get; set; }

		[Required(ErrorMessage = "ProtocolId là bắt buộc")]
		public Guid ProtocolId { get; set; }

		[StringLength(2000, ErrorMessage = "Description không được vượt quá 2000 ký tự")]
		public string? Description { get; set; }
	}

	public class SearchStringDto
	{
		public Guid? SearchStringId { get; set; }

		[Required(ErrorMessage = "StrategyId là bắt buộc")]
		public Guid StrategyId { get; set; }

		[Required(ErrorMessage = "Expression là bắt buộc")]
		[StringLength(5000, ErrorMessage = "Expression không được vượt quá 5000 ký tự")]
		public string Expression { get; set; } = string.Empty;
	}

	public class SearchTermDto
	{
		public Guid? TermId { get; set; }

		[Required(ErrorMessage = "Keyword là bắt buộc")]
		[StringLength(500, ErrorMessage = "Keyword không được vượt quá 500 ký tự")]
		public string Keyword { get; set; } = string.Empty;

		[StringLength(200, ErrorMessage = "Source không được vượt quá 200 ký tự")]
		public string? Source { get; set; }
	}

	public class SearchStringTermDto
	{
		[Required(ErrorMessage = "SearchStringId là bắt buộc")]
		public Guid SearchStringId { get; set; }

		[Required(ErrorMessage = "TermId là bắt buộc")]
		public Guid TermId { get; set; }
	}

	public class SearchSourceDto
	{
		public Guid? SourceId { get; set; }

		[Required(ErrorMessage = "ProtocolId là bắt buộc")]
		public Guid ProtocolId { get; set; }

		[Required(ErrorMessage = "SourceType là bắt buộc")]
		[RegularExpression("^(digital_library|journal|bibliographic_database|conference_proceeding)$",
			ErrorMessage = "SourceType không hợp lệ")]
		public string SourceType { get; set; } = string.Empty;

		[Required(ErrorMessage = "Name là bắt buộc")]
		[StringLength(500, ErrorMessage = "Name không được vượt quá 500 ký tự")]
		public string Name { get; set; } = string.Empty;
	}
}