using System.Collections.Generic;

namespace SRSS.IAM.Services.DTOs.AI
{
    public class PicocAnalysisRequest
    {
        public Guid? SearchSourceId { get; set; }
        public string Population { get; set; } = string.Empty;
        public string Intervention { get; set; } = string.Empty;
        public string Comparator { get; set; } = string.Empty;
        public string Outcome { get; set; } = string.Empty;
        public string Context { get; set; } = string.Empty;
    }

    public class PicocAnalysisResponse
    {
        public List<string> Population { get; set; } = new();
        public List<string> Intervention { get; set; } = new();
        public List<string> Comparison { get; set; } = new();
        public List<string> Outcome { get; set; } = new();
        public List<string> Context { get; set; } = new();
        public string GeneratedQuery { get; set; } = string.Empty;
    }
}
