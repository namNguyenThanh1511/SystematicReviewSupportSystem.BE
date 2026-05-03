using Microsoft.EntityFrameworkCore;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

using SRSS.IAM.Repositories.Entities.Enums;

namespace SRSS.IAM.Repositories.PaperRepo
{
    public interface IPaperRepository : IGenericRepository<Paper, Guid, AppDbContext>
    {
        IQueryable<Paper> GetPapersQueryable(List<Guid> ids);
        Task<Paper?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Paper?> GetByDoiAndProjectAsync(string doi, Guid projectId, CancellationToken cancellationToken = default);
        Task<Paper?> GetByDoiAndIdentificationProcessAsync(string doi, Guid identificationProcessId, CancellationToken cancellationToken = default);
        Task<(List<Paper> Papers, int TotalCount)> GetPapersByProjectAsync(
            Guid projectId,
            string? search,
            SelectionStatus? status,
            int? year,
            string? assignmentStatus,
            ScreeningStage? stage,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Search papers in a project with advanced filtering capabilities
        /// Supports filtering by search text, search strategy ID, search source ID, and publication year
        /// </summary>
        Task<(List<Paper> Papers, int TotalCount)> SearchPapersByProjectAsync(
            Guid projectId,
            string? search,
            Guid? searchStrategyId,
            Guid? searchSourceId,
            int? year,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<(List<Paper> Papers, int TotalCount)> GetPaperPoolByProjectAsync(
            Guid projectId,
            string? searchText,
            string? keyword,
            int? yearFrom,
            int? yearTo,
            Guid? searchSourceId,
            Guid? importBatchId,
            string doiState,
            string fullTextState,
            bool onlyUnused,
            bool recentlyImported,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get duplicate papers for a specific project
        /// Uses DeduplicationResult table for project-scoped results
        /// </summary>
        Task<(List<Paper> Papers, List<DeduplicationResult> Results, int TotalCount)> GetDuplicatePapersByProjectAsync(
            Guid projectId,
            string? search,
            int? year,
            string? sortBy,
            string? sortOrder,
            DeduplicationReviewStatus? reviewStatus,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get unique (non-duplicate) papers for a specific identification process.
        /// Returns papers imported via this process that have no deduplication results.
        /// </summary>
        Task<(List<Paper> Papers, int TotalCount)> GetUniquePapersByIdentificationProcessAsync(
            Guid identificationProcessId,
            string? search,
            int? year,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<(List<Paper> Papers, int TotalCount)> GetPapersMissingExternalDataAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get unique papers for a specific data extraction process.
        /// Returns papers that have an extraction paper task in this process.
        /// </summary>
        Task<(List<Paper> Papers, int TotalCount)> GetUniquePapersByDataExtractionProcessAsync(
            Guid dataExtractionProcessId,
            string? search,
            int? year,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<(List<Paper> Papers, int TotalCount)> GetPapersByIdsAsync(
            List<Guid> paperIds,
            string? search,
            int? year,
            Guid? searchSourceId,
            AssignmentFilterStatus assignmentStatus,
            ResolutionFilterStatus resolutionStatus,
            ScreeningPhase? phase,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<(List<Paper> Papers, int TotalCount)> GetPapersByIdsAsync(
          List<Guid> paperIds,
                    Guid? searchSourceId,
          int pageNumber,
          int pageSize,
          CancellationToken cancellationToken = default);

        Task<List<Paper>> GetTopCitedPapersAsync(int topN, CancellationToken cancellationToken = default);
        Task<List<Paper>> GetPapersWithCitationCountByIdsAsync(IEnumerable<Guid> paperIds, CancellationToken cancellationToken = default);

        Task<IEnumerable<Paper>> FindAllWithEmbeddingAsync(
            System.Linq.Expressions.Expression<Func<Paper, bool>>? predicate = null,
            bool isTracking = true,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get final dataset papers eligible for enrichment after identification completion.
        /// Reuses unique papers logic, filtered to papers with DOI that haven't been enriched yet.
        /// </summary>
        Task<List<Paper>> GetFinalDatasetPapersForEnrichmentAsync(
            Guid identificationProcessId,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<Paper>> FindAllWithLimitAsync(
            System.Linq.Expressions.Expression<Func<Paper, bool>> predicate,
            int limit,
            bool isTracking = true,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get papers with their Quality Assessment details (assignments and decisions)
        /// Avoids N+1 query problem when listing papers in QA process
        /// </summary>
        Task<List<Paper>> GetPapersWithQaDetailsByIdsAsync(
            IEnumerable<Guid> paperIds,
            Guid qaProcessId,
            CancellationToken cancellationToken = default);

        Task<(List<SimplifiedPaperResponse> Items, int TotalCount)> GetSimplifiedPapersAsync(
            List<Guid> paperIds,
            Guid studySelectionProcessId,
            ScreeningPhase phase,
            int requiredReviewers,
            string? search,
            int? year,
            Guid? searchSourceId,
            AssignmentFilterStatus assignmentStatus,
            ResolutionFilterStatus resolutionStatus,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<Paper?> GetForAiEvaluationAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<List<string>> GetExistingDoisByProjectAsync(
            IEnumerable<string> dois,
            Guid projectId,
            CancellationToken cancellationToken = default);

        Task<int> CountAsync(
            System.Linq.Expressions.Expression<Func<Paper, bool>>? predicate = null,
            CancellationToken cancellationToken = default);
    }
}
