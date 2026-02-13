using Shared.Entities.BaseEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Repositories.Entities
{
	public class ResearchQuestion : BaseEntity<Guid>
	{
		public Guid ProjectId { get; set; }
		public Guid QuestionTypeId { get; set; }
		public string QuestionText { get; set; } = string.Empty;
		public string? Rationale { get; set; }

		// Navigation properties
		public SystematicReviewProject Project { get; set; } = null!;
		public QuestionType QuestionType { get; set; } = null!;
		public ICollection<PicocElement> PicocElements { get; set; } = new List<PicocElement>();
	}
}