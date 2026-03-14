namespace SRSS.IAM.Services.DTOs.DataExtraction
{
	/// <summary>
	/// Request complete phase
	/// </summary>
	public class CompletePhaseRequest
	{
		public Guid? CompletedByUserId { get; set; } // Null thì lấy từ JWT
	}

	/// <summary>
	/// Response complete phase
	/// </summary>
	public class CompletePhaseResultDto
	{
		public Guid ReviewProcessId { get; set; }
		public DateTimeOffset CompletedAt { get; set; }
		public int UnresolvedCount { get; set; }
		public bool IsCompleted { get; set; }
	}
}