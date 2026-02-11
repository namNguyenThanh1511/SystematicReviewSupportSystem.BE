using Shared.Entities.BaseEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Repositories.Entities
{
	public class SystematicReviewProject : BaseEntity<Guid>
	{
		public string Title { get; set; } = string.Empty;
		public string? Domain { get; set; }
		public string? Description { get; set; }
		public string Status { get; set; } = "Draft"; // Draft, Active, Completed, Archived
		public DateOnly? StartDate { get; set; }
		public DateOnly? EndDate { get; set; }

		// Navigation properties
		public ICollection<ReviewProtocol> Protocols { get; set; } = new List<ReviewProtocol>();
		public ICollection<ResearchQuestion> ResearchQuestions { get; set; } = new List<ResearchQuestion>();
		public ICollection<ReviewNeed> ReviewNeeds { get; set; } = new List<ReviewNeed>();
		public ICollection<ReviewObjective> ReviewObjectives { get; set; } = new List<ReviewObjective>();
		public ICollection<CommissioningDocument> CommissioningDocuments { get; set; } = new List<CommissioningDocument>();
	}
}
