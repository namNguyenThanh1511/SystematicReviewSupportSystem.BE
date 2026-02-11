using Shared.Entities.BaseEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Repositories.Entities
{
	public class SearchString : BaseEntity<Guid>
	{
		public Guid StrategyId { get; set; }
		public string Expression { get; set; } = string.Empty;

		// Navigation properties
		public SearchStrategy Strategy { get; set; } = null!;
		public ICollection<SearchStringTerm> SearchStringTerms { get; set; } = new List<SearchStringTerm>();
	}
}