using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
    /// <summary>
    /// Represents a duplicate detection result for a specific identification process.
    /// Duplicates are NOT intrinsic properties of papers - they are contextual to a review process.
    /// </summary>
    public class DeduplicationResult : BaseEntity<Guid>
    {
        /// <summary>
        /// The identification process that produced this deduplication result
        /// </summary>
        public Guid IdentificationProcessId { get; set; }

        /// <summary>
        /// The paper identified as a duplicate
        /// </summary>
        public Guid PaperId { get; set; }

        /// <summary>
        /// The original paper that this is a duplicate of
        /// </summary>
        public Guid DuplicateOfPaperId { get; set; }

        /// <summary>
        /// Deduplication method used (e.g., DOI_MATCH, TITLE_FUZZY, HYBRID)
        /// </summary>
        public DeduplicationMethod Method { get; set; }

        /// <summary>
        /// Confidence score of the duplicate match (0.0 to 1.0)
        /// </summary>
        public decimal? ConfidenceScore { get; set; }

        /// <summary>
        /// Optional notes about why this was marked as duplicate
        /// </summary>
        public string? Notes { get; set; }

        // Navigation Properties
        public IdentificationProcess IdentificationProcess { get; set; } = null!;
        public Paper Paper { get; set; } = null!;
        public Paper DuplicateOfPaper { get; set; } = null!;
    }

    /// <summary>
    /// Methods used for detecting duplicates
    /// </summary>
    public enum DeduplicationMethod
    {
        /// <summary>
        /// Exact DOI match (highest confidence)
        /// </summary>
        DOI_MATCH = 0,

        /// <summary>
        /// Fuzzy title matching
        /// </summary>
        TITLE_FUZZY = 1,

        /// <summary>
        /// Title + Author similarity
        /// </summary>
        TITLE_AUTHOR = 2,

        /// <summary>
        /// Hybrid approach (multiple criteria)
        /// </summary>
        HYBRID = 3,

        /// <summary>
        /// Manual review by researcher
        /// </summary>
        MANUAL = 4
    }
}
