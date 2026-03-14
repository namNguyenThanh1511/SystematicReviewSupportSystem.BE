namespace SRSS.IAM.Services.DTOs.DataExtraction
{
	/// <summary>
	/// Request upsert draft
	/// </summary>
	public class UpsertDraftRequest
	{
		public Guid? ReviewerUserId { get; set; } // Null thì lấy từ JWT
		public Guid TemplateId { get; set; }
		public bool IsAutosave { get; set; } = true;
		public List<ExtractionAnswerDto> Answers { get; set; } = new();
	}

	/// <summary>
	/// Response upsert draft
	/// </summary>
	public class DraftUpsertResultDto
	{
		public Guid PaperId { get; set; }
		public Guid ReviewerUserId { get; set; }

		/// <summary>
		/// Status: 0=ToDo, 1=InProgress, 2=AwaitingConsensus, 3=Completed
		/// </summary>
		public int Status { get; set; }

		public int DraftVersion { get; set; }
		public DateTimeOffset UpdatedAt { get; set; }
	}

	/// <summary>
	/// Request submit extraction
	/// </summary>
	public class SubmitExtractionRequest
	{
		public Guid? ReviewerUserId { get; set; } // Null thì lấy từ JWT
		public Guid TemplateId { get; set; }
		public string? SubmissionNote { get; set; }
	}

	/// <summary>
	/// Response submit extraction
	/// </summary>
	public class SubmitExtractionResultDto
	{
		public Guid PaperId { get; set; }

		/// <summary>
		/// Status: 0=ToDo, 1=InProgress, 2=AwaitingConsensus, 3=Completed
		/// </summary>
		public int Status { get; set; }
		public string StatusText { get; set; } = string.Empty;

		public int ConflictCount { get; set; }
		public List<Guid> ConflictFieldIds { get; set; } = new();

		public DateTimeOffset SubmittedAt { get; set; }
	}

	/// <summary>
	/// Request update assignee
	/// </summary>
	public class AssigneeUpdateRequest
	{
		public Guid? AssigneeUserId { get; set; }
	}
}