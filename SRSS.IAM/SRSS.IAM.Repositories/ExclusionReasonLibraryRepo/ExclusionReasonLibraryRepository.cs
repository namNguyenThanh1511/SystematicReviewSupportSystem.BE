using Microsoft.EntityFrameworkCore;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.ExclusionReasonLibraryRepo
{
    public class ExclusionReasonLibraryRepository : GenericRepository<ExclusionReasonLibrary, Guid, AppDbContext>, IExclusionReasonLibraryRepository
    {
        public ExclusionReasonLibraryRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<(List<ExclusionReasonLibrary> Items, int TotalCount)> GetPaginatedAsync(
            string? search,
            bool? onlyActive,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _context.ExclusionReasonLibraries.AsNoTracking();

            if (onlyActive == true)
            {
                query = query.Where(e => e.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(e => 
                    e.Name.ToLower().Contains(searchLower) || 
                    e.Code.ToString().Contains(searchLower));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(e => e.Code)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }
    }
}
