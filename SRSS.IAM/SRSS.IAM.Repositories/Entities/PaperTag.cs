using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
    /// <summary>
    /// A tag applied to a paper within the context of a specific review phase.
    /// Tags are user-defined labels for organizing and filtering papers.
    /// </summary>
    public class PaperTag : BaseEntity<Guid>
    {
        public Guid PaperId { get; set; }

        /// <summary>
        /// The user who applied this tag.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// The review phase this tag belongs to.
        /// </summary>
        public ProcessPhase Phase { get; set; }

        /// <summary>
        /// The tag label text (e.g., "needs-review", "high-priority").
        /// </summary>
        public string Label { get; set; } = string.Empty;

        // Navigation Properties
        public Paper Paper { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
