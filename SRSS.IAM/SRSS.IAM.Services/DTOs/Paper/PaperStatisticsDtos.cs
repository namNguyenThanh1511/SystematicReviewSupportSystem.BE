namespace SRSS.IAM.Services.DTOs.Paper
{
    public class PaperOverviewDto
    {
        public int TotalPapers { get; set; }
        public int TotalPapersWithFulltext { get; set; }
        public double FulltextAvailablePercentage { get; set; }
        public int TotalMissingDoi { get; set; }
        public int TotalMissingAbstract { get; set; }
    }

    public class CountItemDto
    {
        public string Label { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class StatusCountItemDto
    {
        public int Status { get; set; }
        public string Label { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class YearCountDto
    {
        public int Year { get; set; }
        public int Count { get; set; }
    }

    public class DataQualityDto
    {
        public int MissingDoiCount { get; set; }
        public int MissingAbstractCount { get; set; }
        public int MissingAuthorsCount { get; set; }
        public int MissingYearCount { get; set; }
    }

    public class PaperStatisticsFilter
    {
        public int? YearFrom { get; set; }
        public int? YearTo { get; set; }
        public string? Source { get; set; }
    }
}
