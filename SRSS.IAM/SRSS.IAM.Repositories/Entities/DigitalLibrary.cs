using Shared.Entities.BaseEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Repositories.Entities
{
	public class DigitalLibrary : BaseEntity<Guid>
	{
		public Guid SourceId { get; set; }
		public string? AccessUrl { get; set; }

		// Navigation properties
		public SearchSource Source { get; set; } = null!;
	}
}