using Shared.Entities.BaseEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Repositories.Entities
{
	public class ReviewObjective : BaseEntity<Guid>
	{
		public Guid ProjectId { get; set; }
		public string ObjectiveStatement { get; set; } = string.Empty;

		// Navigation properties
		public SystematicReviewProject Project { get; set; } = null!;
	}
}
