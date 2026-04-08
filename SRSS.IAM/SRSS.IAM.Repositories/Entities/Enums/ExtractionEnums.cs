namespace SRSS.IAM.Repositories.Entities
{
	/// <summary>
	/// Chế độ extraction: đơn hoặc kép
	/// </summary>
	public enum ExtractionModeEnum
	{
		SingleExtraction = 0,
		DoubleExtraction = 1
	}

	/// <summary>
	/// Trạng thái của mỗi paper trong quá trình extraction
	/// </summary>
	public enum ExtractionStudyStatusEnum
	{
		ToDo = 0,
		InProgress = 1,
		AwaitingConsensus = 2,
		Completed = 3
	}

	/// <summary>
	/// Loại giá trị answer (phù hợp với FieldType)
	/// </summary>
	public enum ExtractionAnswerValueKindEnum
	{
		Null = 0,
		Text = 1,
		Number = 2,
		Boolean = 3,
		SingleSelect = 4,
		MultiSelect = 5
	}

	/// <summary>
	/// Cách resolve conflict
	/// </summary>
	public enum ConsensusResolutionTypeEnum
	{
		UseSubmissionA = 0,
		UseSubmissionB = 1,
		Manual = 2
	}

	/// <summary>
	/// Loại field trong Extraction Template
	/// </summary>
	public enum FieldType
	{
		Text = 0,
		Integer = 1,
		Decimal = 2,
		Boolean = 3,
		SingleSelect = 4,
		MultiSelect = 5
	}

	/// <summary>
	/// Loại section trong Extraction Template: FlatForm (form thường) hoặc MatrixGrid (ma trận 2D)
	/// </summary>
	public enum SectionType
	{
		FlatForm = 0,
		MatrixGrid = 1
	}

	/// <summary>
	/// Trạng thái của Data Extraction Process
	/// </summary>
	public enum ExtractionProcessStatus
	{
		NotStarted = 0,
		InProgress = 1,
		Completed = 2
	}

	/// <summary>
	/// Trạng thái công việc của Reviewer trong ExtractionPaperTask
	/// </summary>
	public enum ReviewerTaskStatus
	{
		NotStarted = 0,
		InProgress = 1,
		Completed = 2
	}

	/// <summary>
	/// Trạng thái tổng quát của Paper trong ExtractionPaperTask
	/// </summary>
	public enum PaperExtractionStatus
	{
		NotStarted = 0,
		InProgress = 1,
		AwaitingConsensus = 2,
		Completed = 3
	}

	/// <summary>
	/// Target reviewer(s) for reopen/revision request
	/// </summary>
	public enum TargetReviewer
	{
		Reviewer1 = 0,
		Reviewer2 = 1,
		Both = 2,
		Direct = 3 // Leader Direct Extraction: resets the task without touching reviewer state
	}
}