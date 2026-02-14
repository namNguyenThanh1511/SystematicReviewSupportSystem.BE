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
                .FirstOrDefaultAsync(p => p.DOI == doi && p.ProjectId == projectId, cancellationToken);
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

            // Apply status filter
            if (status.HasValue)
            {
                query = query.Where(p => p.CurrentSelectionStatus == status.Value);
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

        public async Task<(List<Paper> Papers, int TotalCount)> GetDuplicatePapersByProjectAsync(
            Guid projectId,
            string? search,
            int? year,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Papers
                .AsNoTracking()
                .Where(p => p.ProjectId == projectId && p.IsDuplicate == true);

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
