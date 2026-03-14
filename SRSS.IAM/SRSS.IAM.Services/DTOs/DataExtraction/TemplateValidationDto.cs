namespace SRSS.IAM.Services.DTOs.DataExtraction
{
	/// <summary>
	/// Validation error details
	/// </summary>
	public class ValidationErrorDetail
	{
		public string Code { get; set; } = string.Empty;
		public string Message { get; set; } = string.Empty;
	}

	/// <summary>
	/// Template validation result
	/// </summary>
	public class TemplateValidationResultDto
	{
		public bool IsValid { get; set; }
		public List<ValidationErrorDetail> Errors { get; set; } = new();
	}
}