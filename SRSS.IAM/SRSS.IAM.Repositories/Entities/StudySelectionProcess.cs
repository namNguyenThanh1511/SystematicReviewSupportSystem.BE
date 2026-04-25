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
        public ICollection<StudySelectionAIResult> StudySelectionAIResults { get; set; } = new List<StudySelectionAIResult>();
        public ICollection<StudySelectionExclusionReason> ExclusionReasons { get; set; } = new List<StudySelectionExclusionReason>();
        public ICollection<StudySelectionCriteria> StudySelectionCriterias { get; set; } = new List<StudySelectionCriteria>();
        public ICollection<StudySelectionCriteriaAIResponse> StudySelectionCriteriaAIResponses { get; set; } = new List<StudySelectionCriteriaAIResponse>();

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

            // if (ReviewProcess.IdentificationProcess.Status != IdentificationStatus.Completed)
            // {
            //     throw new InvalidOperationException("Cannot start study selection before identification process is completed.");
            // }

            Status = SelectionProcessStatus.InProgress;
            StartedAt = DateTimeOffset.UtcNow;
            ModifiedAt = DateTimeOffset.UtcNow;
        }

        public void Complete()
        {
            if (Status != SelectionProcessStatus.InProgress && Status != SelectionProcessStatus.Reopened)
            {
                throw new InvalidOperationException($"Cannot complete selection process from {Status} status.");
            }

            Status = SelectionProcessStatus.Completed;
            CompletedAt = DateTimeOffset.UtcNow;
            ModifiedAt = DateTimeOffset.UtcNow;
        }

        public void Reopen()
        {
            if (Status != SelectionProcessStatus.Completed)
            {
                throw new InvalidOperationException($"Cannot reopen selection process from {Status} status.");
            }

            Status = SelectionProcessStatus.Reopened;
            CompletedAt = null;
            ModifiedAt = DateTimeOffset.UtcNow;
        }
    }

    public enum SelectionProcessStatus
    {
        NotStarted = 0,
        InProgress = 1,
        Completed = 2,
        Reopened = 3
    }
}
