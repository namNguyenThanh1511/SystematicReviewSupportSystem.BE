namespace SRSS.IAM.Services.DTOs.StudySelection
{
    public class StuSeRetrievalQueryDto
    {
        public string QueryType { get; set; } = string.Empty;
        public string QueryText { get; set; } = string.Empty;
        public string? SourceLabel { get; set; }
    }
}
