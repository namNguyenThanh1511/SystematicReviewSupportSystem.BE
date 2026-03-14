namespace SRSS.IAM.Services.DTOs.DataExtraction
{
	/// <summary>
	/// Metrics tổng quát cho extraction phase
	/// </summary>
	public class ExtractionSummaryMetricsDto
	{
		public int TotalIncluded { get; set; }
		public int TodoCount { get; set; }
		public int InProgressCount { get; set; }
		public int AwaitingConsensusCount { get; set; }
		public int CompletedCount { get; set; }
	}

	/// <summary>
	/// Summary template (cho context)
	/// </summary>
	public class ExtractionTemplateSummaryDto
	{
		public Guid TemplateId { get; set; }
		public string Name { get; set; } = string.Empty;
		public string? Description { get; set; }
		public int FieldCount { get; set; }
	}

	/// <summary>
	/// Một item trong danh sách studies
	/// </summary>
	public class DataExtractionStudyListItemDto
	{
		public Guid PaperId { get; set; }
		public string Title { get; set; } = string.Empty;
		public string? Authors { get; set; }
		public int? PublicationYear { get; set; }

		public Guid? AssigneeUserId { get; set; }
		public string? AssigneeName { get; set; }

		/// <summary>
		/// Status: 0=ToDo, 1=InProgress, 2=AwaitingConsensus, 3=Completed
		/// </summary>
		public int Status { get; set; }
		public string StatusText { get; set; } = string.Empty;

		public int ConflictCount { get; set; }
		public bool HasDraft { get; set; }
		public bool HasSubmission { get; set; }

		public DateTimeOffset UpdatedAt { get; set; }
	}

	/// <summary>
	/// Paginated response cho danh sách studies
	/// </summary>
	public class DataExtractionStudyPageDto
	{
		public List<DataExtractionStudyListItemDto> Items { get; set; } = new();
		public int TotalCount { get; set; }
		public int PageNumber { get; set; }
		public int PageSize { get; set; }
		public int TotalPages { get; set; }
		public bool HasPreviousPage { get; set; }
		public bool HasNextPage { get; set; }
	}

	/// <summary>
	/// Chi tiết của 1 study (với drafts, submissions, conflicts)
	/// </summary>
	public class DataExtractionStudyDetailDto
	{
		public DataExtractionStudyListItemDto Study { get; set; } = new();
		public Guid TemplateId { get; set; }

		public List<ReviewerDraftDto> Drafts { get; set; } = new();
		public List<ReviewerSubmissionDto> Submissions { get; set; } = new();

		/// <summary>
		/// Final answers (sau khi resolve conflicts)
		/// </summary>
		public List<ExtractionAnswerDto> FinalAnswers { get; set; } = new();

		/// <summary>
		/// Fields đang có conflict
		/// </summary>
		public List<Guid> ConflictFieldIds { get; set; } = new();

		public bool CanSubmit { get; set; }
		public bool CanResolveConsensus { get; set; }
	}
}