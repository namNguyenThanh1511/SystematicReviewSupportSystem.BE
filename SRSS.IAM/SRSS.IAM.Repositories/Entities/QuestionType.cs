using Shared.Entities.BaseEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Repositories.Entities
{
	public class QuestionType : BaseEntity<Guid>
	{
		public string Name { get; set; } = string.Empty;
		public string? Description { get; set; }

		// Navigation properties
		public ICollection<ResearchQuestion> ResearchQuestions { get; set; } = new List<ResearchQuestion>();
	}
}