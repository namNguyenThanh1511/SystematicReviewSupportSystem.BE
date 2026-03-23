namespace SRSS.IAM.Services.DTOs.DataExtraction
{
    public class ExtractionPreviewDto
    {
        public List<string> Headers { get; set; } = new();
        public List<Dictionary<string, string>> Rows { get; set; } = new();
    }
}
