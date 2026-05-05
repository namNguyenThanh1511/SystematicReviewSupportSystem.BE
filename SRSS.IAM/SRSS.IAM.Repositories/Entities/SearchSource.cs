using Shared.Entities.BaseEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Repositories.Entities
{
	public class SearchSource : BaseEntity<Guid>
	{
		public Guid ProjectId { get; set; }
		public Guid? MasterSourceId { get; set; } // Reference to Master data
    	public string Name { get; set; } = string.Empty; // Backup name or Custom name
		// Navigation properties
		public SystematicReviewProject Project { get; set; } = null!;
		public MasterSearchSources? MasterSource { get; set; }
		public ICollection<SearchStrategy> Strategies { get; set; } = new List<SearchStrategy>();
	}
}
