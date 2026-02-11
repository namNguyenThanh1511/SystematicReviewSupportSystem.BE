using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Services.DTOs.Identification
{
    public class CreateSearchExecutionRequest
    {
        public Guid IdentificationProcessId { get; set; }
        public string SearchSource { get; set; } = string.Empty;
        public string? SearchQuery { get; set; }
        public SearchExecutionType Type { get; set; }
        public string? Notes { get; set; }
    }

    public class SearchExecutionResponse
    {
        public Guid Id { get; set; }
        public Guid IdentificationProcessId { get; set; }
        public string SearchSource { get; set; } = string.Empty;
        public string? SearchQuery { get; set; }
        public DateTimeOffset ExecutedAt { get; set; }
        public int ResultCount { get; set; }
        public SearchExecutionType Type { get; set; }
        public string? Notes { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset ModifiedAt { get; set; }
    }
}
