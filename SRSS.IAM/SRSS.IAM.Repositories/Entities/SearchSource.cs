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
		public string Name { get; set; } = string.Empty;
		// Navigation properties
		public ReviewProtocol Protocol { get; set; } = null!;
	}
}
