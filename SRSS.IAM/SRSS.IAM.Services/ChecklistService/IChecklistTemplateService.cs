using SRSS.IAM.Services.DTOs.Checklist;

namespace SRSS.IAM.Services.ChecklistService
{
    public interface IChecklistTemplateService
    {
        Task<List<ChecklistTemplateSummaryDto>> GetAllTemplatesAsync(bool? isSystem = null, CancellationToken cancellationToken = default);
        Task<List<ChecklistTemplateSummaryDto>> GetSystemTemplatesAsync(CancellationToken cancellationToken = default);
        Task<ChecklistTemplateDetailDto?> GetTemplateByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ChecklistTemplateDetailDto> CreateCustomTemplateAsync(CreateChecklistTemplateDto dto, CancellationToken cancellationToken = default);
        Task<ReviewChecklistDto> CloneTemplateToReviewAsync(Guid templateId, Guid reviewId, CancellationToken cancellationToken = default);
    }
}
