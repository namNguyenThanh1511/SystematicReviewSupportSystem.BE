using Shared.Entities.BaseEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Repositories.Entities
{
	public class InclusionCriterion : BaseEntity<Guid>
	{
		public Guid CriteriaId { get; set; }
		public string Rule { get; set; } = string.Empty;

		// Navigation properties
		public StudySelectionCriteria Criteria { get; set; } = null!;
	}
}
