using Shared.Entities.BaseEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Repositories.Entities
{
	public class SearchStringTerm : BaseEntity<Guid>
	{
		public Guid SearchStringId { get; set; }
		public Guid TermId { get; set; }
		public SearchString SearchString { get; set; }
		public SearchTerm SearchTerm { get; set; }
	}
}
