using Shared.Entities.BaseEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Repositories.Entities
{
	public class SearchSource : BaseEntity<Guid>
	{
		public Guid ProtocolId { get; set; }
		public string SourceType { get; set; } = string.Empty; // DigitalLibrary, Journal, Database, Conference
		public string Name { get; set; } = string.Empty;

		// Navigation properties
		public ReviewProtocol Protocol { get; set; } = null!;
		public DigitalLibrary? DigitalLibrary { get; set; }
		public Journal? Journal { get; set; }
		public BibliographicDatabase? BibliographicDatabase { get; set; }
		public ConferenceProceeding? ConferenceProceeding { get; set; }
	}
}
