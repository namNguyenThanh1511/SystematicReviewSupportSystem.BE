using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
    /// <summary>
    /// The StudySelectionExclusionReason table stores exclusion reasons used within a specific Study Selection Process.
    /// Project leaders can either select from global library or create custom reasons.
    /// </summary>
    public class StudySelectionExclusionReason : BaseEntity<Guid>
    {
        /// <summary>
        /// Foreign key referencing StudySelectionProcess.
        /// </summary>
        public Guid StudySelectionProcessId { get; set; }

        /// <summary>
        /// Optional reference to ExclusionReasonLibraries.
        /// Null if the reason was created manually.
        /// </summary>
        public Guid? LibraryReasonId { get; set; }

        /// <summary>
        /// Numeric identifier of the exclusion reason.
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// Human-readable name of the exclusion reason.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Indicates whether the reason is currently active.
        /// Inactive reasons will not appear when reviewers select exclusion reasons during screening.
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public StudySelectionProcess StudySelectionProcess { get; set; } = null!;
        public ExclusionReasonLibrary? LibraryReason { get; set; }
        public ICollection<ScreeningDecision> ScreeningDecisions { get; set; } = new List<ScreeningDecision>();
        public ICollection<ScreeningResolution> ScreeningResolutions { get; set; } = new List<ScreeningResolution>();
    }
}
