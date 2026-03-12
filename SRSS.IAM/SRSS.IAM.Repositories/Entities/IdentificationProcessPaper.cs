using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
    /// <summary>
    /// Represents the snapshot of papers that survived deduplication
    /// when an IdentificationProcess is completed.
    /// This table is immutable after creation — screening operates on this frozen dataset.
    /// </summary>
    public class IdentificationProcessPaper : BaseEntity<Guid>
    {
        public Guid IdentificationProcessId { get; set; }
        public Guid PaperId { get; set; }
        public bool IncludedAfterDedup { get; set; }

        // Navigation Properties
        public IdentificationProcess IdentificationProcess { get; set; } = null!;
        public Paper Paper { get; set; } = null!;
    }
}
