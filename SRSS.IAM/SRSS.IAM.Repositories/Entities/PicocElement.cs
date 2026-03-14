using Shared.Entities.BaseEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Repositories.Entities
{
	public class PicocElement : BaseEntity<Guid>
	{
		public Guid ResearchQuestionId { get; set; }
		public string ElementType { get; set; } = string.Empty; // Population, Intervention, Comparison, Outcome, Context
		public string Description { get; set; } = string.Empty;

		// Navigation properties
		public ResearchQuestion ResearchQuestion { get; set; } = null!;
		public Population? Population { get; set; }
		public Intervention? Intervention { get; set; }
		public Comparison? Comparison { get; set; }
		public Outcome? Outcome { get; set; }
		public Context? Context { get; set; }
	}
}