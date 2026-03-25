using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Services.DTOs.DataExtraction
{
    public class DataExtractionProcessResponse
    {
        public Guid Id { get; set; }
        public Guid ReviewProcessId { get; set; }
        public ExtractionProcessStatus Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public DateTimeOffset? StartedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
        public string? Notes { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset ModifiedAt { get; set; }
    }
}