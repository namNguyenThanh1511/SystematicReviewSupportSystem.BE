using SRSS.IAM.Services.DTOs.Identification;
using SRSS.IAM.Services.DTOs.Paper;
using Microsoft.EntityFrameworkCore;
using SRSS.IAM.Repositories.Entities.Enums;

namespace SRSS.IAM.Services.IdentificationService
{
    public interface IIdentificationService
    {
        Task<IdentificationProcessResponse> CreateIdentificationProcessAsync(CreateIdentificationProcessRequest request, CancellationToken cancellationToken = default);
        Task<IdentificationProcessResponse> GetIdentificationProcessByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IdentificationProcessResponse> StartIdentificationProcessAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IdentificationProcessResponse> CompleteIdentificationProcessAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IdentificationProcessResponse> ReopenIdentificationProcessAsync(Guid id, CancellationToken cancellationToken = default);

        Task<SearchExecutionResponse> CreateSearchExecutionAsync(CreateSearchExecutionRequest request, CancellationToken cancellationToken = default);
        Task<SearchExecutionResponse> GetSearchExecutionByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<SearchExecutionResponse>> GetSearchExecutionsByIdentificationProcessIdAsync(Guid identificationProcessId, CancellationToken cancellationToken = default);
        Task<SearchExecutionResponse> UpdateSearchExecutionAsync(UpdateSearchExecutionRequest request, CancellationToken cancellationToken = default);
        Task<bool> DeleteSearchExecutionAsync(Guid id, CancellationToken cancellationToken = default);

        Task<PrismaStatisticsResponse> GetPrismaStatisticsAsync(Guid identificationProcessId, CancellationToken cancellationToken = default);

        Task<ImportBatchResponse> CreateImportBatchAsync(CreateImportBatchRequest request, CancellationToken cancellationToken = default);
        Task<ImportBatchResponse> GetImportBatchByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<ImportBatchResponse>> GetImportBatchesBySearchExecutionIdAsync(Guid searchExecutionId, CancellationToken cancellationToken = default);
        Task<List<ImportBatchResponse>> GetImportBatchesByIdentificationProcessIdAsync(Guid identificationProcessId, CancellationToken cancellationToken = default);
        Task<ImportBatchResponse> UpdateImportBatchAsync(UpdateImportBatchRequest request, CancellationToken cancellationToken = default);
        Task<bool> DeleteImportBatchAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<PaperResponse>> GetPapersByImportBatchIdAsync(Guid importBatchId, CancellationToken cancellationToken = default);

        Task<ImportPaperResponse> ImportPaperAsync(ImportPaperRequest request, CancellationToken cancellationToken = default);
        Task<RisImportResultDto> ImportRisFileAsync(
            Stream fileStream, 
            string fileName,
            Guid? searchSourceId,
            string? importedBy,
            Guid? searchExecutionId,
            Guid identificationProcessId,
            CancellationToken cancellationToken = default);

        Task MarkAsDuplicateAsync(
            Guid identificationProcessId,
            Guid paperId,
            MarkAsDuplicateRequest request,
            CancellationToken cancellationToken = default);

        Task<(List<PaperResponse> Papers, int TotalCount)> GetReadyPapersForSnapshotAsync(
            Guid identificationProcessId,
            string? search,
            int? year,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task AddPapersToIdentificationSnapshotAsync(
            Guid identificationProcessId,
            List<Guid> paperIds,
            CancellationToken cancellationToken = default);

        Task<(List<PaperResponse> Papers, int TotalCount)> GetPaperIdentificationProcessSnapshotAsync(
            Guid identificationProcessId,
            string? search,
            int? year,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);
    }
}


