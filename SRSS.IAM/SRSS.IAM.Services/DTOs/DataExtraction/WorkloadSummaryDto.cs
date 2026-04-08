namespace SRSS.IAM.Services.DTOs.DataExtraction
{
    public class ReviewerWorkloadDto
    {
        public Guid ReviewerId { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
        public int TotalAssigned { get; set; }
        public int Completed { get; set; }
        public int InProgress { get; set; }
        public int NotStarted { get; set; }
    }

    public class ExtractionWorkloadSummaryDto
    {
        public int TotalPapers { get; set; }
        public int FullyCompletedPapers { get; set; }
        public double OverallProgressPercentage { get; set; }
        public List<ReviewerWorkloadDto> ReviewerWorkloads { get; set; } = new();
    }
}
