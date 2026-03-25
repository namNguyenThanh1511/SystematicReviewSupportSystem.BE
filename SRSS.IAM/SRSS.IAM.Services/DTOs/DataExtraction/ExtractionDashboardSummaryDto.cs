namespace SRSS.IAM.Services.DTOs.DataExtraction
{
	public class ExtractionDashboardSummaryDto
	{
		public int TotalIncluded { get; set; }
		public int InProgress { get; set; }
		public int AwaitingConsensus { get; set; }
		public int Completed { get; set; }
	}
}
