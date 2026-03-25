namespace SRSS.IAM.Services.DTOs.DataExtraction
{
	public class ExtractionDashboardResponseDto
	{
		public ExtractionDashboardSummaryDto Summary { get; set; } = new ExtractionDashboardSummaryDto();
		public PaginatedList<ExtractionDashboardTaskDto> Tasks { get; set; } = new PaginatedList<ExtractionDashboardTaskDto>();
	}

	public class PaginatedList<T>
	{
		public List<T> Items { get; set; } = new List<T>();
		public int TotalCount { get; set; }
		public int PageNumber { get; set; }
		public int PageSize { get; set; }

		public PaginatedList() { }

		public PaginatedList(List<T> items, int count, int pageNumber, int pageSize)
		{
			Items = items;
			TotalCount = count;
			PageNumber = pageNumber;
			PageSize = pageSize;
		}
	}
}
