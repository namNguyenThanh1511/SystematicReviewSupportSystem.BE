using Shared.Entities.BaseEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Repositories.Entities
{
	public class SearchTerm : BaseEntity<Guid>
	{
		public string Keyword { get; set; } = string.Empty;
		public string? Source { get; set; }

		// Navigation properties
		public ICollection<SearchStringTerm> SearchStringTerms { get; set; } = new List<SearchStringTerm>();
	}
}
