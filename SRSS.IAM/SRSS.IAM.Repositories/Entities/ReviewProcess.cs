using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
    public class ReviewProcess : BaseEntity<Guid>
    {
        public Guid ProjectId { get; set; }
        public ProcessStatus Status { get; set; } = ProcessStatus.Pending;
        public DateTimeOffset? StartedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
        public ProcessPhase CurrentPhase { get; set; } = ProcessPhase.Identification;
        public string? Notes { get; set; }

        // Navigation Properties
        public SystematicReviewProject Project { get; set; } = null!;

        public IdentificationProcess? IdentificationProcess { get; set; }
        public StudySelectionProcess? StudySelectionProcess { get; set; }
        public ICollection<PrismaReport> PrismaReports { get; set; } = new List<PrismaReport>();

        // Domain Methods
        public void Start()
        {
            if (Status != ProcessStatus.Pending)
            {
                throw new InvalidOperationException($"Cannot start process from {Status} status.");
            }

            if (Project.ReviewProcesses.Any(rp => rp.Id != Id && rp.Status == ProcessStatus.InProgress))
            {
                throw new InvalidOperationException("Cannot start this process while another process is in progress.");
            }

            Status = ProcessStatus.InProgress;
            StartedAt = DateTimeOffset.UtcNow;
            ModifiedAt = DateTimeOffset.UtcNow;
        }

        public void Complete()
        {
            if (Status != ProcessStatus.InProgress)
            {
                throw new InvalidOperationException($"Cannot complete process from {Status} status.");
            }

            EnsureCanComplete();

            Status = ProcessStatus.Completed;
            CompletedAt = DateTimeOffset.UtcNow;
            ModifiedAt = DateTimeOffset.UtcNow;
        }

        public void Cancel()
        {
            if (Status == ProcessStatus.Completed)
            {
                throw new InvalidOperationException("Cannot cancel a completed process.");
            }

            Status = ProcessStatus.Cancelled;
            ModifiedAt = DateTimeOffset.UtcNow;
        }

        public bool CanStart()
        {
            if (Status != ProcessStatus.Pending)
            {
                return false;
            }

            return !Project.ReviewProcesses.Any(rp => rp.Id != Id && rp.Status == ProcessStatus.InProgress);
        }

        public void EnsureCanCreateIdentificationProcess()
        {
            if (Status != ProcessStatus.InProgress && Status != ProcessStatus.Pending)
            {
                throw new InvalidOperationException($"Cannot create identification process when review process status is {Status}.");
            }

            if (IdentificationProcess != null)
            {
                throw new InvalidOperationException("IdentificationProcess already exists for this ReviewProcess.");
            }
        }

        public void EnsureCanCreateStudySelectionProcess()
        {
            if (Status != ProcessStatus.InProgress)
            {
                throw new InvalidOperationException($"Cannot create study selection process when review process status is {Status}.");
            }

            if (IdentificationProcess == null)
            {
                throw new InvalidOperationException("Cannot create StudySelectionProcess before IdentificationProcess exists.");
            }

            if (IdentificationProcess.Status != IdentificationStatus.Completed)
            {
                throw new InvalidOperationException("Cannot create StudySelectionProcess before IdentificationProcess is completed.");
            }

            if (StudySelectionProcess != null)
            {
                throw new InvalidOperationException("StudySelectionProcess already exists for this ReviewProcess.");
            }
        }

        public void EnsureCanComplete()
        {
            if (StudySelectionProcess == null || StudySelectionProcess.Status != SelectionProcessStatus.Completed)
            {
                throw new InvalidOperationException("Cannot complete ReviewProcess before StudySelectionProcess is completed.");
            }
        }
    }

    public enum ProcessStatus
    {
        Pending = 0,
        InProgress = 1,
        Completed = 2,
        Cancelled = 3
    }

    public enum ProcessPhase
    {
        Identification = 0,
        Screening = 1,
        DataExtraction = 2,
        QualityAssessment = 3,
        Synthesis = 4
    }
}
