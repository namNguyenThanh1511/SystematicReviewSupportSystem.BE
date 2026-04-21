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
        Task<PrismaStatisticsResponse> GetPrismaStatisticsAsync(Guid identificationProcessId, CancellationToken cancellationToken = default);

        Task<ImportBatchResponse> CreateImportBatchAsync(CreateImportBatchRequest request, CancellationToken cancellationToken = default);
        Task<ImportBatchResponse> GetImportBatchByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<ImportBatchResponse>> GetImportBatchesByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
        Task<List<PaperResponse>> GetPapersByImportBatchIdAsync(Guid importBatchId, CancellationToken cancellationToken = default);

        Task<RisImportResultDto> ImportRisFileAsync(
            Stream fileStream, 
            string fileName,
            Guid? searchSourceId,
            Guid projectId,
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
            Guid? searchSourceId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        // Task AddPapersToIdentificationSnapshotAsync(
        //     Guid identificationProcessId,
        //     List<Guid> paperIds,
        //     CancellationToken cancellationToken = default);

        Task<(List<PaperResponse> Papers, int TotalCount)> GetPaperIdentificationProcessSnapshotAsync(
            Guid identificationProcessId,
            string? search,
            int? year,
            Guid? searchSourceId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);
    }
}


