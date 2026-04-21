using Microsoft.EntityFrameworkCore;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.DeduplicationResultRepo
{
    public class DeduplicationResultRepository : GenericRepository<DeduplicationResult, Guid, AppDbContext>, IDeduplicationResultRepository
    {
        public DeduplicationResultRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<DeduplicationResult>> GetByIdentificationProcessAsync(
            Guid identificationProcessId,
            CancellationToken cancellationToken = default)
        {
            var projectId = await ResolveProjectIdAsync(identificationProcessId, cancellationToken);
            if (projectId == Guid.Empty)
            {
                return new List<DeduplicationResult>();
            }

            return await _context.DeduplicationResults
                .AsNoTracking()
                .Where(dr => dr.ProjectId == projectId)
                .OrderBy(dr => dr.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<DeduplicationResult?> GetByPaperAndProcessAsync(
            Guid paperId,
            Guid identificationProcessId,
            CancellationToken cancellationToken = default)
        {
            var projectId = await ResolveProjectIdAsync(identificationProcessId, cancellationToken);
            if (projectId == Guid.Empty)
            {
                return null;
            }

            return await _context.DeduplicationResults
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    dr => dr.PaperId == paperId && dr.ProjectId == projectId,
                    cancellationToken);
        }

        public async Task<int> CountDuplicatesByProcessAsync(
            Guid identificationProcessId,
            CancellationToken cancellationToken = default)
        {
            var projectId = await ResolveProjectIdAsync(identificationProcessId, cancellationToken);
            if (projectId == Guid.Empty)
            {
                return 0;
            }

            // Only count active duplicates (pending or confirmed CANCEL).
            return await _context.DeduplicationResults
                .Where(dr => dr.ProjectId == projectId
                    && (dr.ReviewStatus == DeduplicationReviewStatus.Pending
                        || dr.ResolvedDecision == DuplicateResolutionDecision.CANCEL))
                .CountAsync(cancellationToken);
        }

        public async Task<(List<DeduplicationResult> Results, int TotalCount)> GetDuplicatePairsAsync(
            Guid identificationProcessId,
            string? search,
            DeduplicationReviewStatus? status,
            decimal? minConfidence,
            DeduplicationMethod? method,
            string? sortBy,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var projectId = await ResolveProjectIdAsync(identificationProcessId, cancellationToken);
            if (projectId == Guid.Empty)
            {
                return (new List<DeduplicationResult>(), 0);
            }

            var query = _context.DeduplicationResults
                .AsNoTracking()
                .Include(dr => dr.Paper)
                .Include(dr => dr.DuplicateOfPaper)
                .Where(dr => dr.ProjectId == projectId && !dr.Paper.IsDeleted && !dr.DuplicateOfPaper.IsDeleted);

            // Filter by review status
            if (status.HasValue)
            {
                query = query.Where(dr => dr.ReviewStatus == status.Value);
            }

            // Filter by minimum confidence
            if (minConfidence.HasValue)
            {
                query = query.Where(dr => dr.ConfidenceScore >= minConfidence.Value);
            }

            // Filter by detection method
            if (method.HasValue)
            {
                query = query.Where(dr => dr.Method == method.Value);
            }

            // Search across BOTH papers in the pair
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(dr =>
                    (dr.Paper.Title != null && dr.Paper.Title.ToLower().Contains(searchLower)) ||
                    (dr.Paper.DOI != null && dr.Paper.DOI.ToLower().Contains(searchLower)) ||
                    (dr.Paper.Authors != null && dr.Paper.Authors.ToLower().Contains(searchLower)) ||
                    (dr.DuplicateOfPaper.Title != null && dr.DuplicateOfPaper.Title.ToLower().Contains(searchLower)) ||
                    (dr.DuplicateOfPaper.DOI != null && dr.DuplicateOfPaper.DOI.ToLower().Contains(searchLower)) ||
                    (dr.DuplicateOfPaper.Authors != null && dr.DuplicateOfPaper.Authors.ToLower().Contains(searchLower)));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            // Apply sorting
            query = sortBy?.ToLower() switch
            {
                "confidenceasc" => query.OrderBy(dr => dr.ConfidenceScore),
                "detectedatdesc" => query.OrderByDescending(dr => dr.CreatedAt),
                _ => query.OrderByDescending(dr => dr.ConfidenceScore) // default: confidenceDesc
            };

            var results = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (results, totalCount);
        }

        private async Task<Guid> ResolveProjectIdAsync(Guid identificationProcessId, CancellationToken cancellationToken)
        {
            return await _context.IdentificationProcesses
                .AsNoTracking()
                .Where(ip => ip.Id == identificationProcessId)
                .Select(ip => ip.ReviewProcess.ProjectId)
                .SingleOrDefaultAsync(cancellationToken);
        }
    }
}
