using SRSS.IAM.Services.DTOs.Identification;
using SRSS.IAM.Services.DTOs.Paper;
using SRSS.IAM.Services.DTOs.Crossref;
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
        Task<PrismaStatisticsResponse> GetPrismaStatisticsAsync(Guid reviewProcessId, CancellationToken cancellationToken = default);

        Task<ImportBatchResponse> CreateImportBatchAsync(CreateImportBatchRequest request, CancellationToken cancellationToken = default);
        Task<ImportBatchResponse> GetImportBatchByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<ImportBatchResponse>> GetImportBatchesByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
        Task<List<PaperResponse>> GetPapersByImportBatchIdAsync(Guid importBatchId, CancellationToken cancellationToken = default);

        // ── Import entry points ───────────────────────────────────────────────────

        /// <summary>Parses a RIS file stream and runs the full import pipeline.</summary>
        Task<RisImportResultDto> ImportRisFileAsync(
            Stream fileStream,
            string fileName,
            Guid? searchSourceId,
            Guid projectId,
            CancellationToken cancellationToken = default);

        /// <summary>Parses a BibTeX file stream and runs the full import pipeline.</summary>
        Task<RisImportResultDto> ImportBibTexFileAsync(
            Stream fileStream,
            string fileName,
            Guid? searchSourceId,
            Guid projectId,
            CancellationToken cancellationToken = default);

        /// <summary>Resolves a single DOI via Crossref and runs the full import pipeline.</summary>
        Task<RisImportResultDto> ImportFromDoiAsync(
            string doi,
            Guid? searchSourceId,
            Guid projectId,
            CancellationToken cancellationToken = default);

        /// <summary>Queries Crossref with the given parameters and runs the full import pipeline.</summary>
        Task<RisImportResultDto> ImportFromApiAsync(
            CrossrefQueryParameters query,
            Guid? searchSourceId,
            Guid projectId,
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


