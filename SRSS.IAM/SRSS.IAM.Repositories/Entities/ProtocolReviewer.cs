using Shared.Entities.BaseEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Repositories.Entities
{
	public class ProtocolReviewer : BaseEntity<Guid>
	{
		public string Name { get; set; } = string.Empty;
		public string? Role { get; set; }
		public string? Affiliation { get; set; }

		// Navigation properties
		public ICollection<ProtocolEvaluation> Evaluations { get; set; } = new List<ProtocolEvaluation>();
	}
}