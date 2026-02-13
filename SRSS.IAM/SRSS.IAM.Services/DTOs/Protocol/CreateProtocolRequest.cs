using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Services.DTOs.Protocol
{
	public class CreateProtocolRequest
	{
		[Required(ErrorMessage = "ProjectId là bắt buộc")]
		public Guid ProjectId { get; set; }

		[StringLength(50, ErrorMessage = "Protocol version không được vượt quá 50 ký tự")]
		public string ProtocolVersion { get; set; } = "1.0";

		public string? Description { get; set; }
	}
}
