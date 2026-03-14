namespace SRSS.IAM.Services.DTOs.DataExtraction
{
	/// <summary>
	/// 1 item trong consensus queue
	/// </summary>
	public class ConsensusQueueItemDto
	{
		public Guid PaperId { get; set; }
		public string Title { get; set; } = string.Empty;
		public int ConflictCount { get; set; }
		public int SubmissionCount { get; set; }
		public DateTimeOffset UpdatedAt { get; set; }
	}

	/// <summary>
	/// Paginated consensus queue
	/// </summary>
	public class ConsensusQueuePageDto
	{
		public List<ConsensusQueueItemDto> Items { get; set; } = new();
		public int TotalCount { get; set; }
		public int PageNumber { get; set; }
		public int PageSize { get; set; }
		public int TotalPages { get; set; }
		public bool HasPreviousPage { get; set; }
		public bool HasNextPage { get; set; }
	}

	/// <summary>
	/// 1 field đang conflict (side-by-side compare)
	/// </summary>
	public class ConsensusConflictFieldDto
	{
		public Guid FieldId { get; set; }
		public string FieldName { get; set; } = string.Empty;

		/// <summary>
		/// Field type: 0=Text, 1=Integer, 2=Decimal, 3=Boolean, 4=SingleSelect, 5=MultiSelect
		/// </summary>
		public int FieldType { get; set; }

		public ExtractionAnswerValueDto SubmissionAValue { get; set; } = new();
		public ExtractionAnswerValueDto SubmissionBValue { get; set; } = new();
	}

	/// <summary>
	/// Chi tiết consensus (2 submissions side-by-side)
	/// </summary>
	public class ConsensusDetailDto
	{
		public DataExtractionStudyListItemDto Study { get; set; } = new();
		public ReviewerSubmissionDto SubmissionA { get; set; } = new();
		public ReviewerSubmissionDto SubmissionB { get; set; } = new();
		public List<ConsensusConflictFieldDto> ConflictFields { get; set; } = new();
	}

	/// <summary>
	/// Resolution cho 1 field đang conflict
	/// </summary>
	public class ConsensusFieldResolutionDto
	{
		public Guid FieldId { get; set; }

		/// <summary>
		/// Type: 0=UseSubmissionA, 1=UseSubmissionB, 2=Manual
		/// </summary>
		public int ResolutionType { get; set; }

		/// <summary>
		/// Giá trị manual (nếu ResolutionType = 2)
		/// </summary>
		public ExtractionAnswerValueDto? ManualValue { get; set; }
	}

	/// <summary>
	/// Request resolve consensus
	/// </summary>
	public class ResolveConsensusRequest
	{
		public Guid? ResolverUserId { get; set; } // Null thì lấy từ JWT
		public string? ResolutionNote { get; set; }
		public List<ConsensusFieldResolutionDto> FieldResolutions { get; set; } = new();
	}
}