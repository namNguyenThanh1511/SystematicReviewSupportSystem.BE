using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
    /// <summary>
    /// Represents a duplicate detection result at project scope.
    /// Duplicates are NOT intrinsic properties of papers - they are contextual to a project dataset.
    /// </summary>
    public class DeduplicationResult : BaseEntity<Guid>
    {
        /// <summary>
        /// The project that produced this deduplication result
        /// </summary>
        public Guid ProjectId { get; set; }

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
        public decimal ConfidenceScore { get; set; }

        /// <summary>
        /// Optional notes about why this was marked as duplicate
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Review status of this deduplication result (Pending, Confirmed, Rejected)
        /// </summary>
        public DeduplicationReviewStatus ReviewStatus { get; set; } = DeduplicationReviewStatus.Pending;

        /// <summary>
        /// Who reviewed this deduplication result
        /// </summary>
        public string? ReviewedBy { get; set; }

        /// <summary>
        /// When this deduplication result was reviewed
        /// </summary>
        public DateTimeOffset? ReviewedAt { get; set; }

        /// <summary>
        /// The resolution decision made by the reviewer.
        /// CANCEL = PaperId is excluded as duplicate.
        /// KEEP_BOTH = Not a real duplicate, both papers remain.
        /// </summary>
        public DuplicateResolutionDecision? ResolvedDecision { get; set; }

        // Navigation Properties
        public SystematicReviewProject Project { get; set; } = null!;
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
        MANUAL = 4,

        /// <summary>
        /// AI-based semantic similarity
        /// </summary>
        SEMANTIC = 5
    }

    /// <summary>
    /// Review status for a deduplication result
    /// </summary>
    public enum DeduplicationReviewStatus
    {
        /// <summary>
        /// Not yet reviewed by a researcher
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Researcher confirmed this is a duplicate
        /// </summary>
        Confirmed = 1,

        /// <summary>
        /// Researcher rejected — not actually a duplicate, keep both papers
        /// </summary>
        Rejected = 2
    }

    /// <summary>
    /// Resolution decision for a duplicate pair.
    /// Replaces the old survivor-chain model (keep-original/keep-duplicate/keep-both).
    /// Each pair is independent — no cascade logic.
    /// </summary>
    public enum DuplicateResolutionDecision
    {
        /// <summary>
        /// Not a real duplicate — both papers remain in the dataset
        /// </summary>
        KEEP_BOTH = 0,

        /// <summary>
        /// Confirmed duplicate — PaperId is excluded from the identification process.
        /// DuplicateOfPaperId is just the reference paper, NOT a "survivor".
        /// </summary>
        CANCEL = 1
    }
}
