using Shared.Entities.BaseEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Repositories.Entities
{
	public class CommissioningDocument : BaseEntity<Guid>
	{
		public Guid ProjectId { get; set; }
		public string? Sponsor { get; set; }
		public string? Scope { get; set; }
		public decimal? Budget { get; set; }
		public string? DocumentUrl { get; set; }

		// Navigation properties
		public SystematicReviewProject Project { get; set; } = null!;
	}
}