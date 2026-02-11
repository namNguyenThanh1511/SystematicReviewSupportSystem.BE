using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Services.DTOs.SearchStrategy
{
	public class CreateSearchStrategyRequest
	{
		[Required(ErrorMessage = "ProtocolId là bắt buộc")]
		public Guid ProtocolId { get; set; }

		public string? Description { get; set; }

		public List<CreateSearchStringRequest> SearchStrings { get; set; } = new();
	}

	public class CreateSearchStringRequest
	{
		[Required(ErrorMessage = "Expression là bắt buộc")]
		public string Expression { get; set; } = string.Empty;

		public List<Guid> TermIds { get; set; } = new();
	}
}