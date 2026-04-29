using Microsoft.AspNetCore.Http;
using SRSS.IAM.Services.DTOs.Checklist;

namespace SRSS.IAM.Services.ChecklistService
{
    public interface IReviewChecklistService
    {
        Task<List<ReviewChecklistSummaryDto>> GetReviewChecklistsAsync(Guid reviewId, CancellationToken cancellationToken = default);
        Task<ReviewChecklistDto?> GetChecklistByIdAsync(Guid checkListId, CancellationToken cancellationToken = default);
        Task<ChecklistItemResponseDto> UpdateItemResponseAsync(Guid reviewChecklistId, Guid itemId, UpdateChecklistItemDto dto, CancellationToken cancellationToken = default);
        Task<ChecklistCompletionDto> CalculateCompletionPercentageAsync(Guid reviewChecklistId, CancellationToken cancellationToken = default);
        Task<ReviewChecklistDto?> GetChecklistForReportAsync(Guid checkListId, CancellationToken cancellationToken = default);
        Task<byte[]> GenerateReportAsync(Guid checkListId, GenerateReportRequest request, CancellationToken cancellationToken = default);
        Task<ChecklistAutoFillStatusDto> QueueAutoFillChecklist(Guid checkListId, IFormFile file, CancellationToken cancellationToken = default);
        Task AutoFillChecklistFromPdfAsync(Guid checkListId, Stream pdfStream, string fileName, Guid userId, CancellationToken cancellationToken = default);
    }
}
