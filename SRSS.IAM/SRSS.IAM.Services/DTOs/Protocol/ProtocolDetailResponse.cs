using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Services.DTOs.Protocol
{
	public class ProtocolDetailResponse
	{
		public Guid ProtocolId { get; set; }
		public Guid ProjectId { get; set; }
		public string ProtocolVersion { get; set; } = string.Empty;
		public string Status { get; set; } = string.Empty;
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? ApprovedAt { get; set; }
		public List<VersionHistoryDto> Versions { get; set; } = new();
	}

	public class VersionHistoryDto
	{
		public Guid VersionId { get; set; }
		public string VersionNumber { get; set; } = string.Empty;
		public string? ChangeSummary { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
	}
}