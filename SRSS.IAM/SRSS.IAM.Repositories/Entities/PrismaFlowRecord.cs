using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
    /// <summary>
    /// Represents one box/stage in the PRISMA 2020 flow diagram
    /// </summary>
    public class PrismaFlowRecord : BaseEntity<Guid>
    {
        /// <summary>
        /// Report this record belongs to
        /// </summary>
        public Guid PrismaReportId { get; set; }

        /// <summary>
        /// PRISMA stage this record represents
        /// </summary>
        public PrismaStage Stage { get; set; }

        /// <summary>
        /// Display label for this stage
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Count/value for this stage (snapshot)
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Optional description or breakdown
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Display order in the flow diagram
        /// </summary>
        public int DisplayOrder { get; set; }

        // Navigation Properties
        public PrismaReport PrismaReport { get; set; } = null!;
    }
}
