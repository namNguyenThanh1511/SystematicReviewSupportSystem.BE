using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
    /// <summary>
    /// Represents a snapshot of PRISMA 2020 flow diagram data for a review process
    /// </summary>
    public class PrismaReport : BaseEntity<Guid>
    {
        /// <summary>
        /// Review process this report belongs to
        /// </summary>
        public Guid ReviewProcessId { get; set; }

        /// <summary>
        /// Report version (e.g., "1.0", "1.1")
        /// </summary>
        public string Version { get; set; } = "1.0";

        /// <summary>
        /// When this report was generated
        /// </summary>
        public DateTimeOffset GeneratedAt { get; set; }

        /// <summary>
        /// Optional notes about this report generation
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Who generated this report
        /// </summary>
        public string? GeneratedBy { get; set; }

        // Navigation Properties
        public ReviewProcess ReviewProcess { get; set; } = null!;
        public ICollection<PrismaFlowRecord> FlowRecords { get; set; } = new List<PrismaFlowRecord>();
    }
}
