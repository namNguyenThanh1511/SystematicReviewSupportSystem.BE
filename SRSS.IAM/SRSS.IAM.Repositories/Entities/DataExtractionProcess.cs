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
	}
}
