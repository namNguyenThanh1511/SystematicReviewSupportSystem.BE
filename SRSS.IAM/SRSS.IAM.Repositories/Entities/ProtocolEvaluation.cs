using Shared.Entities.BaseEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Repositories.Entities
{
	public class ProtocolEvaluation : BaseEntity<Guid>
	{
		public Guid ProtocolId { get; set; }
		public Guid ReviewerId { get; set; }
		public string EvaluationResult { get; set; } = string.Empty; // Approved, Rejected, NeedsRevision
		public string? Comment { get; set; }
		public DateTimeOffset EvaluatedAt { get; set; } = DateTimeOffset.UtcNow;

		// Navigation properties
		public ReviewProtocol Protocol { get; set; } = null!;
		public ProtocolReviewer Reviewer { get; set; } = null!;
	}
}
