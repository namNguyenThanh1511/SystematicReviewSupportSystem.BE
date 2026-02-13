using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
	public class ProjectTimetable : BaseEntity<Guid>
	{
		public Guid ProtocolId { get; set; }
		public string Milestone { get; set; } = string.Empty;
		public DateOnly? PlannedDate { get; set; }

		// Navigation properties
		public ReviewProtocol Protocol { get; set; } = null!;
	}
}