using Shared.Entities.BaseEntity;
using SRSS.IAM.Repositories.Entities.Enums;

namespace SRSS.IAM.Repositories.Entities
{
    /// <summary>
    /// Represents the Full-Text screening phase (Phase 2) of study selection.
    /// Placeholder entity — will be fleshed out in a future iteration.
    /// Child entity of StudySelectionProcess (1:1 relationship).
    /// </summary>
    public class FullTextScreening : BaseEntity<Guid>
    {
        public Guid StudySelectionProcessId { get; set; }
        public ScreeningPhaseStatus Status { get; set; } = ScreeningPhaseStatus.NotStarted;
        public DateTimeOffset? StartedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
        public int MinReviewersPerPaper { get; set; } = 2;
        public int MaxReviewersPerPaper { get; set; } = 3;

        // Navigation Properties
        public StudySelectionProcess StudySelectionProcess { get; set; } = null!;

        // Domain Methods
        public void Start()
        {
            if (Status != ScreeningPhaseStatus.NotStarted)
            {
                throw new InvalidOperationException($"Cannot start Full-Text screening from {Status} status.");
            }

            Status = ScreeningPhaseStatus.InProgress;
            StartedAt = DateTimeOffset.UtcNow;
            ModifiedAt = DateTimeOffset.UtcNow;
        }

        public void Complete()
        {
            if (Status != ScreeningPhaseStatus.InProgress)
            {
                throw new InvalidOperationException($"Cannot complete Full-Text screening from {Status} status.");
            }

            Status = ScreeningPhaseStatus.Completed;
            CompletedAt = DateTimeOffset.UtcNow;
            ModifiedAt = DateTimeOffset.UtcNow;
        }
    }
}
