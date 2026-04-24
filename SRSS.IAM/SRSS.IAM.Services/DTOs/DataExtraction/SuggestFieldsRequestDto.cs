namespace SRSS.IAM.Services.DTOs.DataExtraction
{
    public class SuggestFieldsRequestDto
    {
        public string SectionName { get; set; } = string.Empty;
        public string? ProjectContext { get; set; }
    }
}
