using Shared.Entities.BaseEntity;
using SRSS.IAM.Repositories.Entities.Enums;

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
        public ScreeningPhase CurrentPhase { get; set; } = ScreeningPhase.TitleAbstract;

        // Navigation Properties
        public ReviewProcess ReviewProcess { get; set; } = null!;
        public TitleAbstractScreening? TitleAbstractScreening { get; set; }
        public FullTextScreening? FullTextScreening { get; set; }
        public ICollection<ScreeningDecision> ScreeningDecisions { get; set; } = new List<ScreeningDecision>();
        public ICollection<ScreeningResolution> ScreeningResolutions { get; set; } = new List<ScreeningResolution>();
        public ICollection<StudySelectionProcessPaper> StudySelectionProcessPapers { get; set; } = new List<StudySelectionProcessPaper>();
        public ICollection<PaperAssignment> PaperAssignments { get; set; } = new List<PaperAssignment>();

        // Domain Methods
        public void Start()
        {
            if (Status != SelectionProcessStatus.NotStarted)
            {
                throw new InvalidOperationException($"Cannot start selection process from {Status} status.");
            }

            if (ReviewProcess.IdentificationProcess == null)
            {
                throw new InvalidOperationException("Cannot start study selection before identification process exists.");
            }

            if (ReviewProcess.IdentificationProcess.Status != IdentificationStatus.Completed)
            {
                throw new InvalidOperationException("Cannot start study selection before identification process is completed.");
            }

            Status = SelectionProcessStatus.InProgress;
            StartedAt = DateTimeOffset.UtcNow;
            ModifiedAt = DateTimeOffset.UtcNow;
        }

        public void Complete()
        {
            if (Status != SelectionProcessStatus.InProgress)
            {
                throw new InvalidOperationException($"Cannot complete selection process from {Status} status.");
            }

            Status = SelectionProcessStatus.Completed;
            CompletedAt = DateTimeOffset.UtcNow;
            ModifiedAt = DateTimeOffset.UtcNow;
        }
    }

    public enum SelectionProcessStatus
    {
        NotStarted = 0,
        InProgress = 1,
        Completed = 2
    }
}
