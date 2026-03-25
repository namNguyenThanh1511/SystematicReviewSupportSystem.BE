using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
	public class ExtractionPaperTask : BaseEntity<Guid>
	{
		public Guid DataExtractionProcessId { get; set; }
		public Guid PaperId { get; set; }
		public Guid? Reviewer1Id { get; set; }
		public Guid? Reviewer2Id { get; set; }
		public Guid? AdjudicatorId { get; set; }
		public ReviewerTaskStatus Reviewer1Status { get; set; }
		public ReviewerTaskStatus Reviewer2Status { get; set; }
		public PaperExtractionStatus Status { get; set; }

		public DataExtractionProcess DataExtractionProcess { get; set; } = null!;
		public Paper Paper { get; set; } = null!;
		public User? Reviewer1 { get; set; }
		public User? Reviewer2 { get; set; }
		public User? Adjudicator { get; set; }
	}
}
