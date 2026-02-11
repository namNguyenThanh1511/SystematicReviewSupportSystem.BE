using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Repositories.Entities
{
	public class SearchStringTerm
	{
		public Guid SearchStringId { get; set; }
		public Guid TermId { get; set; }

		// Navigation properties
		public SearchString SearchString { get; set; } = null!;
		public SearchTerm SearchTerm { get; set; } = null!;
	}
}
