using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
    /// <summary>
    /// Represents an individual reviewer's decision on a paper during screening
    /// Supports multi-reviewer workflow
    /// </summary>
    public class ScreeningDecision : BaseEntity<Guid>
    {
        public Guid StudySelectionProcessId { get; set; }
        public Guid PaperId { get; set; }
        public Guid ReviewerId { get; set; }
        public ScreeningDecisionType Decision { get; set; }
        public string? Reason { get; set; }
        public DateTimeOffset DecidedAt { get; set; }

        // Navigation Properties
        public StudySelectionProcess StudySelectionProcess { get; set; } = null!;
        public Paper Paper { get; set; } = null!;
    }

    public enum ScreeningDecisionType
    {
        Include = 0,
        Exclude = 1
    }
}
