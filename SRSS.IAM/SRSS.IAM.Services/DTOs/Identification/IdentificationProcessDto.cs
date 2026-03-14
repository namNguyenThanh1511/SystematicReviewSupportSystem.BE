using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Services.DTOs.Identification
{
    public class CreateIdentificationProcessRequest
    {
        public Guid ReviewProcessId { get; set; }
        public string? Notes { get; set; }
    }

    public class IdentificationProcessResponse
    {
        public Guid Id { get; set; }
        public Guid ReviewProcessId { get; set; }
        public string? Notes { get; set; }
        public DateTimeOffset? StartedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
        public IdentificationStatus Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset ModifiedAt { get; set; }

        public PrismaStatisticsResponse PrismaStatistics { get; set; }
    }
}
