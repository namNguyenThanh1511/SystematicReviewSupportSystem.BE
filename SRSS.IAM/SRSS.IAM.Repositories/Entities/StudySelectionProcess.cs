using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
    /// <summary>
    /// Represents the study selection (screening) phase of a systematic review
    /// </summary>
    public class StudySelectionProcess : BaseEntity<Guid>
    {
        public Guid ReviewProcessId { get; set; }
        public string? Notes { get; set; }
        public DateTimeOffset? StartedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
        public SelectionProcessStatus Status { get; set; } = SelectionProcessStatus.NotStarted;

        // Navigation Properties
        public ReviewProcess ReviewProcess { get; set; } = null!;
        public ICollection<ScreeningDecision> ScreeningDecisions { get; set; } = new List<ScreeningDecision>();
        public ICollection<ScreeningResolution> ScreeningResolutions { get; set; } = new List<ScreeningResolution>();
    }

    public enum SelectionProcessStatus
    {
        NotStarted = 0,
        InProgress = 1,
        Completed = 2
    }
}
