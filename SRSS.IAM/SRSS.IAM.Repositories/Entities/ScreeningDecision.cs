using Shared.Entities.BaseEntity;
using SRSS.IAM.Repositories.Entities.Enums;

namespace SRSS.IAM.Repositories.Entities
{
    /// <summary>
    /// Represents an individual reviewer's decision on a paper during screening
    /// Supports multi-reviewer workflow with phase discrimination
    /// </summary>
    public class ScreeningDecision : BaseEntity<Guid>
    {
        public Guid StudySelectionProcessId { get; set; }
        public Guid PaperId { get; set; }
        public Guid ReviewerId { get; set; }
        public ScreeningDecisionType Decision { get; set; }
        public ScreeningPhase Phase { get; set; } = ScreeningPhase.TitleAbstract;
        public Guid? ExclusionReasonId { get; set; }
        public string? Reason { get; set; }
        public DateTimeOffset DecidedAt { get; set; }

        // Navigation Properties
        public StudySelectionProcess StudySelectionProcess { get; set; } = null!;
        public Paper Paper { get; set; } = null!;
        public StudySelectionExclusionReason? ExclusionReason { get; set; }
        public StudySelectionChecklistSubmission? ChecklistSubmission { get; set; }
    }

    public enum ScreeningDecisionType
    {
        Include = 0,
        Exclude = 1
    }
}
