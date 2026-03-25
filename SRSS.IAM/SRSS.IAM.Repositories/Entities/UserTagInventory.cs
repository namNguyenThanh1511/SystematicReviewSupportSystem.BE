using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
    /// <summary>
    /// A user's personal tag inventory entry — a tag definition that a user has created or used,
    /// scoped to a specific review phase. Used as a suggestion source and usage tracker.
    /// </summary>
    public class UserTagInventory : BaseEntity<Guid>
    {
        public Guid UserId { get; set; }

        /// <summary>
        /// The review phase this tag is associated with.
        /// </summary>
        public ProcessPhase Phase { get; set; }

        /// <summary>
        /// The tag label text.
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Number of times this tag has been applied by this user across all papers.
        /// </summary>
        public int UsageCount { get; set; } = 0;

        // Navigation Properties
        public User User { get; set; } = null!;
    }
}
