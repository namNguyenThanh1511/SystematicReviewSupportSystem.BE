using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
	public class DataExtractionProcess : BaseEntity<Guid>
	{
		public Guid ReviewProcessId { get; set; }
		public string? Notes { get; set; }
		public DateTimeOffset? StartedAt { get; set; }
		public DateTimeOffset? CompletedAt { get; set; }
		public ExtractionProcessStatus Status { get; set; }

		public ReviewProcess ReviewProcess { get; set; } = null!;
		public ICollection<ExtractionPaperTask> ExtractionPaperTasks { get; set; } = new List<ExtractionPaperTask>();

		// Domain Methods
		public void Start()
		{
			if (Status != ExtractionProcessStatus.NotStarted)
			{
				throw new InvalidOperationException($"Cannot start extraction process from {Status} status.");
			}

			Status = ExtractionProcessStatus.InProgress;
			StartedAt = DateTimeOffset.UtcNow;
			ModifiedAt = DateTimeOffset.UtcNow;
		}

		public void Complete()
		{
			if (Status != ExtractionProcessStatus.InProgress && Status != ExtractionProcessStatus.Reopened)
			{
				throw new InvalidOperationException($"Cannot complete extraction process from {Status} status.");
			}

			Status = ExtractionProcessStatus.Completed;
			CompletedAt = DateTimeOffset.UtcNow;
			ModifiedAt = DateTimeOffset.UtcNow;
		}

		public void Reopen()
		{
			if (Status != ExtractionProcessStatus.Completed)
			{
				throw new InvalidOperationException($"Cannot reopen extraction process from {Status} status.");
			}

			Status = ExtractionProcessStatus.Reopened;
			CompletedAt = null;
			ModifiedAt = DateTimeOffset.UtcNow;
		}
	}
}
