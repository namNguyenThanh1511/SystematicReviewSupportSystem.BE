using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
    public class IdentificationProcess : BaseEntity<Guid>
    {
        public Guid ReviewProcessId { get; set; }
        public string? Notes { get; set; }
        public DateTimeOffset? StartedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
        public IdentificationStatus Status { get; set; }

        public ICollection<SearchExecution> SearchExecutions { get; set; } = new List<SearchExecution>();
        public ICollection<DeduplicationResult> DeduplicationResults { get; set; } = new List<DeduplicationResult>();
        public ReviewProcess ReviewProcess { get; set; } = null!;

        // Domain Methods
        public void Start()
        {
            if (Status != IdentificationStatus.NotStarted)
            {
                throw new InvalidOperationException($"Cannot start identification process from {Status} status.");
            }

            Status = IdentificationStatus.InProgress;
            StartedAt = DateTimeOffset.UtcNow;
            ModifiedAt = DateTimeOffset.UtcNow;
        }

        public void Complete()
        {
            if (Status != IdentificationStatus.InProgress)
            {
                throw new InvalidOperationException($"Cannot complete identification process from {Status} status.");
            }

            Status = IdentificationStatus.Completed;
            CompletedAt = DateTimeOffset.UtcNow;
            ModifiedAt = DateTimeOffset.UtcNow;
        }
    }

    public enum IdentificationStatus
    {
        NotStarted = 0,
        InProgress = 1,
        Completed = 2
    }
}
