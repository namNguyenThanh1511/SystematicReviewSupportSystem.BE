using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
    /// <summary>
    /// The ExclusionReasonLibrary table stores a global library of exclusion reasons 
    /// that can be reused across different projects and screening processes.
    /// These reasons serve as standard templates for defining why a study or paper 
    /// should be excluded during the screening phases.
    /// </summary>
    public class ExclusionReasonLibrary : BaseEntity<Guid>
    {
        /// <summary>
        /// Numeric code representing the exclusion reason.
        /// Used for standardized identification and ordering of reasons.
        /// Should be unique within the library.
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// Human-readable name of the exclusion reason.
        /// Displayed in the user interface when reviewers select a reason.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Indicates whether the reason is currently available for use.
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
