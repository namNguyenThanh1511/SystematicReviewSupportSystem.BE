using Shared.Entities.BaseEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Repositories.Entities
{
	public class Population : BaseEntity<Guid>
	{
		public Guid PicocId { get; set; }
		public string Description { get; set; } = string.Empty;

		// Navigation properties
		public PicocElement PicocElement { get; set; } = null!;
	}
}