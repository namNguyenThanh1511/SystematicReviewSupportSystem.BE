using Shared.Entities.BaseEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Repositories.Entities
{
	public class BibliographicDatabase : BaseEntity<Guid>
	{
		public Guid SourceId { get; set; }

		// Navigation properties
		public SearchSource Source { get; set; } = null!;
	}
}
