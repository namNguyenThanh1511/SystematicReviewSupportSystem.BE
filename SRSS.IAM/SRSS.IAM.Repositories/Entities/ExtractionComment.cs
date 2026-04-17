using System;
using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
	public class ExtractionComment : BaseEntity<Guid>
	{
		public Guid ExtractionPaperTaskId { get; set; }
		public Guid FieldId { get; set; }
		public Guid? MatrixColumnId { get; set; }
		public int? MatrixRowIndex { get; set; }
		public Guid ThreadOwnerId { get; set; }
		public Guid UserId { get; set; }
		public string Content { get; set; } = null!;

		// Navigations
		public ExtractionPaperTask ExtractionPaperTask { get; set; } = null!;
		public ExtractionField Field { get; set; } = null!;
		public ExtractionMatrixColumn? MatrixColumn { get; set; }
		public User User { get; set; } = null!;
	}
}
