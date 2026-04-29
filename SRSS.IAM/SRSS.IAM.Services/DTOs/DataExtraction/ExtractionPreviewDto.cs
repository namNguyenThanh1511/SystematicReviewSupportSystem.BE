namespace SRSS.IAM.Services.DTOs.DataExtraction
{
    public class ExtractionPreviewDto
    {
        public List<ExtractionGridColumnMetaDto> Columns { get; set; } = new();
        public List<Dictionary<string, string>> Rows { get; set; } = new();
    }
}
