using Shared.Entities.BaseEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Repositories.Entities
{
	public class StudySelectionCriteria : BaseEntity<Guid>
	{
		public Guid ProjectId { get; set; }
		public string Description { get; set; } = string.Empty;

		// Navigation properties
		public SystematicReviewProject Project { get; set; } = null!;
		public ICollection<InclusionCriterion> InclusionCriteria { get; set; } = new List<InclusionCriterion>();
		public ICollection<ExclusionCriterion> ExclusionCriteria { get; set; } = new List<ExclusionCriterion>();
	}
}
