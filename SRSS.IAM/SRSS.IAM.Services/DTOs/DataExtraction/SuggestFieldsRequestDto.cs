using System.Collections.Generic;

namespace SRSS.IAM.Services.DTOs.DataExtraction
{
    public class SuggestFieldsRequestDto
    {
        public string SectionName { get; set; } = string.Empty;
        public string? ProjectContext { get; set; }
    }

    public class SuggestFieldsResponseDto
    {
        public List<ExtractionFieldDto> DataExtractionFields { get; set; } = new();
    }
}
