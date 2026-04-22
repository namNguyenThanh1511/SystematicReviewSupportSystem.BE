using Shared.Entities.BaseEntity;
using SRSS.IAM.Repositories.Entities.Enums;

namespace SRSS.IAM.Repositories.Entities
{
	public class DataSynthesisStrategy : BaseEntity<Guid>
	{
		public Guid ProjectId { get; set; }
		public SynthesisType SynthesisType { get; set; }
		public string? Description { get; set; }

		public List<Guid> TargetResearchQuestionIds { get; set; } = new List<Guid>();
		public string? DataGroupingPlan { get; set; }
		public string? SensitivityAnalysisPlan { get; set; }

		// Navigation properties
		public SystematicReviewProject Project { get; set; } = null!;
	}
}