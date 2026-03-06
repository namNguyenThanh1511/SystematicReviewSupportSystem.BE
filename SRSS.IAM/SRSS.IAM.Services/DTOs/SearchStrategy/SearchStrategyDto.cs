using System.ComponentModel.DataAnnotations;

namespace SRSS.IAM.Services.DTOs.SearchStrategy
{

	public class SearchSourceDto
	{
		public Guid? SourceId { get; set; }

		[Required(ErrorMessage = "ProtocolId là bắt buộc")]
		public Guid ProtocolId { get; set; }

		[Required(ErrorMessage = "Name là bắt buộc")]
		[StringLength(500, ErrorMessage = "Name không được vượt quá 500 ký tự")]
		public string Name { get; set; } = string.Empty;
	}
}