using SRSS.IAM.Services.DTOs.DataExtraction;

namespace SRSS.IAM.Services.DataExtractionService
{
    public interface IDataExtractionService
    {
        // ==================== Extraction Templates with Tree Structure ====================

        Task<ExtractionTemplateDto> UpsertTemplateAsync(ExtractionTemplateDto dto);

        Task<List<ExtractionTemplateDto>> GetTemplatesByProcessIdAsync(Guid processId);

        Task<ExtractionTemplateDto> GetTemplateByIdAsync(Guid templateId);

        Task DeleteTemplateAsync(Guid templateId);

        /// <summary>
        /// Validate template structure without saving to database
        /// </summary>
        Task<TemplateValidationResultDto> ValidateTemplateAsync(ExtractionTemplateDto dto);

        /// <summary>
        /// AI suggests extraction fields based on section name/context (RQ)
        /// </summary>
        Task<List<ExtractionFieldDto>> SuggestFieldsForSectionAsync(string sectionName, string projectContext = "");
    }
}