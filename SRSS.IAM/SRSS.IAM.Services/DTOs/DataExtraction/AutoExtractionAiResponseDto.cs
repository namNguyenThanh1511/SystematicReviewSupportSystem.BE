using System.Text.Json.Serialization;

namespace SRSS.IAM.Services.DTOs.DataExtraction
{
    public class AutoExtractionAiResponseDto
    {
        /// <summary>
        /// List of extracted values. We support multiple names because different AI models 
        /// might choose different wrapper keys in JSON mode.
        /// </summary>
        [JsonPropertyName("ExtractedData")]
        public List<ExtractedValueDto> ExtractedData { get; set; } = new();

        [JsonIgnore]
        [JsonPropertyName("results")]
        public List<ExtractedValueDto> Results { get => ExtractedData; set => ExtractedData = value; }
    }
}
