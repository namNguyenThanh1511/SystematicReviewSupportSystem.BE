using Shared.Entities.BaseEntity;
using System.ComponentModel.DataAnnotations;

namespace SRSS.IAM.Repositories.Entities
{
	/// <summary>
	/// Biểu mẫu trích xuất dữ liệu (Template)
	/// Trong 1 protocol có thể có nhiều template
	/// </summary>
	public class ExtractionTemplate : BaseEntity<Guid>
	{
		public Guid ProtocolId { get; set; }

		[MaxLength(500)]
		public string Name { get; set; } = string.Empty;

		[MaxLength(2000)]
		public string? Description { get; set; }

		// Navigation properties
		public ReviewProtocol Protocol { get; set; } = null!;
		public ICollection<ExtractionField> Fields { get; set; } = new List<ExtractionField>();
	}
}