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
		public Guid ProtocolId { get; set; }
		public Guid? MasterSourceId { get; set; } // Reference to Master data
    	public string Name { get; set; } = string.Empty; // Backup name or Custom name
		// Navigation properties
		public ReviewProtocol Protocol { get; set; } = null!;
		public MasterSearchSources? MasterSource { get; set; }
	}
}
