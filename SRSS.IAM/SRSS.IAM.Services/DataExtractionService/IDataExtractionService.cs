using SRSS.IAM.Services.DTOs.DataExtraction;

namespace SRSS.IAM.Services.DataExtractionService
{
	public interface IDataExtractionService
	{
		// ==================== Extraction Templates with Tree Structure ====================

		Task<ExtractionTemplateDto> UpsertTemplateAsync(ExtractionTemplateDto dto);

		Task<List<ExtractionTemplateDto>> GetTemplatesByProtocolIdAsync(Guid protocolId);

		Task<ExtractionTemplateDto> GetTemplateByIdAsync(Guid templateId);

		Task DeleteTemplateAsync(Guid templateId);

		/// <summary>
		/// Validate template structure without saving to database
		/// </summary>
		Task<TemplateValidationResultDto> ValidateTemplateAsync(ExtractionTemplateDto dto);
	}
}