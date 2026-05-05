using System.ComponentModel.DataAnnotations;

namespace SRSS.IAM.Services.DTOs.SearchStrategy
{
	public class SearchSourceDto
	{
		public Guid? SourceId { get; set; }
		public Guid? MasterSourceId { get; set; }

		[Required(ErrorMessage = "ProjectId là bắt buộc")]
		public Guid ProjectId { get; set; }

		[Required(ErrorMessage = "Name là bắt buộc")]
		[StringLength(500, ErrorMessage = "Name không được vượt quá 500 ký tự")]
		public string Name { get; set; } = string.Empty;

		public string? Url { get; set; } // Derived from MasterSource.BaseUrl

		public List<SearchStrategyDto> Strategies { get; set; } = new();
	}

	public class SearchStrategyDto
	{
		public Guid? Id { get; set; }
		public string Query { get; set; } = string.Empty;
		public string[] Fields { get; set; } = Array.Empty<string>();

		// Keywords breakdown
		public string[] PopulationKeywords { get; set; } = Array.Empty<string>();
		public string[] InterventionKeywords { get; set; } = Array.Empty<string>();
		public string[] ComparisonKeywords { get; set; } = Array.Empty<string>();
		public string[] OutcomeKeywords { get; set; } = Array.Empty<string>();
		public string[] ContextKeywords { get; set; } = Array.Empty<string>();

		public SearchFiltersDto Filters { get; set; } = new();
		public DateTimeOffset? DateSearched { get; set; }
		public string? Version { get; set; }
		public string? Notes { get; set; }
	}

	public class SearchFiltersDto
	{
		public int? YearFrom { get; set; }
		public int? YearTo { get; set; }
		public string? Language { get; set; }
		public string? StudyType { get; set; }
	}
}