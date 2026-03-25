namespace SRSS.IAM.Services.DTOs.DataExtraction
{
	public class ExtractionDashboardFilterDto
	{
		public string? SearchQuery { get; set; }
		public string? StatusFilter { get; set; }
		public int PageNumber { get; set; } = 1;
		public int PageSize { get; set; } = 10;
	}
}
