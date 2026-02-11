using Shared.Entities.BaseEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Repositories.Entities
{
	public class SearchStrategy : BaseEntity<Guid>
	{
		public Guid ProtocolId { get; set; }
		public string? Description { get; set; }

		// Navigation properties
		public ReviewProtocol Protocol { get; set; } = null!;
		public ICollection<SearchString> SearchStrings { get; set; } = new List<SearchString>();
	}
}