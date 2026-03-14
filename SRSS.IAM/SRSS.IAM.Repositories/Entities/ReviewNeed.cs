using Shared.Entities.BaseEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Repositories.Entities
{
	public class ReviewNeed : BaseEntity<Guid>
	{
		public Guid ProjectId { get; set; }
		public string Description { get; set; } = string.Empty;
		public string? Justification { get; set; }
		public string? IdentifiedBy { get; set; }

		// Navigation properties
		public SystematicReviewProject Project { get; set; } = null!;
	}
}