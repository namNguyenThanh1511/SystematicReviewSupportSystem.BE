using Microsoft.EntityFrameworkCore;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.PaperRepo
{
    public class PaperRepository : GenericRepository<Paper, Guid, AppDbContext>, IPaperRepository
    {
        public PaperRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Paper?> GetByDoiAndProjectAsync(string doi, Guid projectId, CancellationToken cancellationToken = default)
        {
            return await _context.Papers
                .Where(p => p.DOI == doi && p.ProjectId == projectId && !p.IsRemovedAsDuplicate)
                .OrderBy(p => p.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Paper?> GetByDoiAndSearchExecutionAsync(string doi, Guid searchExecutionId, CancellationToken cancellationToken = default)
        {
            return await _context.Papers
                .Where(p => p.DOI == doi
                    && p.ImportBatch != null
                    && p.ImportBatch.SearchExecutionId == searchExecutionId
                    && !p.IsRemovedAsDuplicate)
                .OrderBy(p => p.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<(List<Paper> Papers, int TotalCount)> GetPapersByProjectAsync(
            Guid projectId,
            string? search,
            SelectionStatus? status,
            int? year,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Papers
                .AsNoTracking()
                .Where(p => p.ProjectId == projectId);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(p =>
                    (p.Title != null && p.Title.ToLower().Contains(searchLower)) ||
                    (p.DOI != null && p.DOI.ToLower().Contains(searchLower)) ||
                    (p.Authors != null && p.Authors.ToLower().Contains(searchLower)));
            }

            // Status filtering removed - status is process-scoped, not paper-scoped
            // Use ScreeningResolution table for status queries

            // Apply year filter
            if (year.HasValue)
            {
                query = query.Where(p => p.PublicationYearInt == year.Value);
            }

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply ordering and pagination
            var papers = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (papers, totalCount);
        }

        /// <summary>
        /// Get duplicate papers for a specific identification process
        /// Queries DeduplicationResult table for process-scoped duplicates
        /// </summary>
        public async Task<(List<Paper> Papers, List<DeduplicationResult> Results, int TotalCount)> GetDuplicatePapersByIdentificationProcessAsync(
            Guid identificationProcessId,
            string? search,
            int? year,
            string? sortBy,
            string? sortOrder,
            DeduplicationReviewStatus? reviewStatus,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            // Query deduplication results for this identification process
            var query = _context.DeduplicationResults
                .AsNoTracking()
                .Include(dr => dr.Paper)
                .Include(dr => dr.DuplicateOfPaper)
                .Where(dr => dr.IdentificationProcessId == identificationProcessId);

            // Apply review status filter
            if (reviewStatus.HasValue)
            {
                query = query.Where(dr => dr.ReviewStatus == reviewStatus.Value);
            }

            // Apply search filter on paper metadata
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(dr =>
                    (dr.Paper.Title != null && dr.Paper.Title.ToLower().Contains(searchLower)) ||
                    (dr.Paper.DOI != null && dr.Paper.DOI.ToLower().Contains(searchLower)) ||
                    (dr.Paper.Authors != null && dr.Paper.Authors.ToLower().Contains(searchLower)));
            }

            // Apply year filter
            if (year.HasValue)
            {
                query = query.Where(dr => dr.Paper.PublicationYearInt == year.Value);
            }

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply sorting
            var isDescending = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);

            query = sortBy?.ToLower() switch
            {
                "confidencescore" => isDescending
                    ? query.OrderByDescending(dr => dr.ConfidenceScore)
                    : query.OrderBy(dr => dr.ConfidenceScore),
                "title" => isDescending
                    ? query.OrderByDescending(dr => dr.Paper.Title)
                    : query.OrderBy(dr => dr.Paper.Title),
                "method" => isDescending
                    ? query.OrderByDescending(dr => dr.Method)
                    : query.OrderBy(dr => dr.Method),
                "reviewstatus" => isDescending
                    ? query.OrderByDescending(dr => dr.ReviewStatus)
                    : query.OrderBy(dr => dr.ReviewStatus),
                _ => query.OrderByDescending(dr => dr.CreatedAt) // default: detectedAt DESC
            };

            // Apply pagination
            var deduplicationResults = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            // Extract papers from results
            var papers = deduplicationResults.Select(dr => dr.Paper).ToList();

            return (papers, deduplicationResults, totalCount);
        }

        /// <summary>
        /// Get unique (non-duplicate) papers for a specific identification process.
        /// Papers linked via ImportBatch → SearchExecution → IdentificationProcess
        /// that are not removed as duplicates and have no pending deduplication results.
        /// </summary>
        public async Task<(List<Paper> Papers, int TotalCount)> GetUniquePapersByIdentificationProcessAsync(
            Guid identificationProcessId,
            string? search,
            int? year,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Papers
                .AsNoTracking()
                .Where(p =>
                    p.ImportBatch != null &&
                    p.ImportBatch.SearchExecution != null &&
                    p.ImportBatch.SearchExecution.IdentificationProcessId == identificationProcessId &&
                    !p.IsRemovedAsDuplicate &&
                    // Also exclude papers with pending (unresolved) deduplication
                    !p.DuplicateResults.Any(dr =>
                        dr.IdentificationProcessId == identificationProcessId &&
                        dr.ReviewStatus == DeduplicationReviewStatus.Pending));

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(p =>
                    (p.Title != null && p.Title.ToLower().Contains(searchLower)) ||
                    (p.DOI != null && p.DOI.ToLower().Contains(searchLower)) ||
                    (p.Authors != null && p.Authors.ToLower().Contains(searchLower)));
            }

            // Apply year filter
            if (year.HasValue)
            {
                query = query.Where(p => p.PublicationYearInt == year.Value);
            }

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply ordering and pagination
            var papers = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (papers, totalCount);
        }
    }
}
