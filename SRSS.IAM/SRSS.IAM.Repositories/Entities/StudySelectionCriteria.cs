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
		public Guid ProtocolId { get; set; }
		public string? Description { get; set; }

		// Navigation properties
		public ReviewProtocol Protocol { get; set; } = null!;
		public ICollection<InclusionCriterion> InclusionCriteria { get; set; } = new List<InclusionCriterion>();
		public ICollection<ExclusionCriterion> ExclusionCriteria { get; set; } = new List<ExclusionCriterion>();
	}
}
