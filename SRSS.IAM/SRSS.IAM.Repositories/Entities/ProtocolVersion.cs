using Shared.Entities.BaseEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Repositories.Entities
{
	public class ProtocolVersion : BaseEntity<Guid>
	{
		public Guid ProtocolId { get; set; }
		public string VersionNumber { get; set; } = string.Empty;
		public string? ChangeSummary { get; set; }
		public string SnapshotData { get; set; } = string.Empty; // JSON snapshot of protocol at this version

		// Navigation properties
		public ReviewProtocol Protocol { get; set; } = null!;
	}
}