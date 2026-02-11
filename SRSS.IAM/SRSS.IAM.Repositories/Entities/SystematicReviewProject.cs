using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
    public class SystematicReviewProject : BaseEntity<Guid>
    {
        public string Title { get; set; } = string.Empty;
        public string? Domain { get; set; }
        public string? Description { get; set; }
        public ProjectStatus Status { get; set; } = ProjectStatus.Draft;
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }

        public Guid OwnerId { get; set; }

        // Navigation Properties
        public ICollection<ReviewProcess> ReviewProcesses { get; set; } = new List<ReviewProcess>();

        // Domain Methods
        public void Activate()
        {
            if (Status != ProjectStatus.Draft)
            {
                throw new InvalidOperationException($"Cannot activate project from {Status} status.");
            }

            Status = ProjectStatus.Active;
            StartDate = DateTimeOffset.UtcNow;
            ModifiedAt = DateTimeOffset.UtcNow;
        }

        public void Complete()
        {
            if (Status != ProjectStatus.Active)
            {
                throw new InvalidOperationException($"Cannot complete project from {Status} status.");
            }

            if (ReviewProcesses.Any(rp => rp.Status == ProcessStatus.InProgress))
            {
                throw new InvalidOperationException("Cannot complete project while processes are in progress.");
            }

            if (!ReviewProcesses.All(rp => rp.Status == ProcessStatus.Completed))
            {
                throw new InvalidOperationException("Cannot complete project until all processes are completed.");
            }

            Status = ProjectStatus.Completed;
            EndDate = DateTimeOffset.UtcNow;
            ModifiedAt = DateTimeOffset.UtcNow;
        }

        public void Archive()
        {
            if (Status != ProjectStatus.Active && Status != ProjectStatus.Completed)
            {
                throw new InvalidOperationException($"Cannot archive project from {Status} status.");
            }

            Status = ProjectStatus.Archived;
            ModifiedAt = DateTimeOffset.UtcNow;
        }

        public ReviewProcess AddReviewProcess(string? notes = null)
        {
            if (Status == ProjectStatus.Completed || Status == ProjectStatus.Archived)
            {
                throw new InvalidOperationException($"Cannot add processes to project in {Status} status.");
            }


            if (ReviewProcesses.Any(rp => rp.Status == ProcessStatus.InProgress))
            {
                throw new InvalidOperationException("Cannot add a new process while another process is in progress.");
            }

            var reviewProcess = new ReviewProcess
            {
                Id = Guid.NewGuid(),
                ProjectId = Id,
                Status = ProcessStatus.Pending,
                Notes = notes,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            };

            ReviewProcesses.Add(reviewProcess);
            ModifiedAt = DateTimeOffset.UtcNow;

            return reviewProcess;
        }

        public bool CanAddProcess()
        {
            if (Status == ProjectStatus.Completed || Status == ProjectStatus.Archived)
            {
                return false;
            }

            return !ReviewProcesses.Any(rp => rp.Status == ProcessStatus.InProgress);
        }
    }

    public enum ProjectStatus
    {
        Draft = 0,
        Active = 1,
        Completed = 2,
        Archived = 3
    }
}
