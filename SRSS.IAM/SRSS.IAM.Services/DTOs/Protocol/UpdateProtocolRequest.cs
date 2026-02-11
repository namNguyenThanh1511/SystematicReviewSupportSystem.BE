using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Services.DTOs.Protocol
{
	public class UpdateProtocolRequest
	{
		[Required(ErrorMessage = "ProtocolId là bắt buộc")]
		public Guid ProtocolId { get; set; }

		public string? Description { get; set; }

		[StringLength(1000, ErrorMessage = "Change summary không được vượt quá 1000 ký tự")]
		public string? ChangeSummary { get; set; }
	}
}
