using Shared.Entities.BaseEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Repositories.Entities
{
	public class StudyCharacteristics : BaseEntity<Guid>
	{
		public Guid ProtocolId { get; set; }
		public string? Language { get; set; }
		public string? Domain { get; set; }
		public string? StudyType { get; set; }

		// Navigation properties
		public ReviewProtocol Protocol { get; set; } = null!;
	}
}
