using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
	public class ProjectTimetable : BaseEntity<Guid>
	{
		public Guid ProjectId { get; set; }
		public string Milestone { get; set; } = string.Empty;
		public DateTimeOffset? PlannedDate { get; set; }

		// Navigation properties
		public SystematicReviewProject Project { get; set; } = null!;
	}
}