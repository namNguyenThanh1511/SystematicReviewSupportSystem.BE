using Microsoft.EntityFrameworkCore;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.SearchExecutionRepo
{
    public class SearchExecutionRepository : GenericRepository<SearchExecution, Guid, AppDbContext>, ISearchExecutionRepository
    {
        public SearchExecutionRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<SearchExecution?> GetByIdWithProjectAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.SearchExecutions
                .Include(se => se.IdentificationProcess)
                    .ThenInclude(ip => ip.ReviewProcess)
                .Include(se => se.SearchSource)
                .FirstOrDefaultAsync(se => se.Id == id, cancellationToken);
        }

        public async Task<SearchExecution?> GetByIdWithSourceAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.SearchExecutions
                .Include(se => se.SearchSource)
                .FirstOrDefaultAsync(se => se.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<SearchExecution>> GetByProcessIdWithSourceAsync(Guid processId, CancellationToken cancellationToken = default)
        {
            return await _context.SearchExecutions
                .Include(se => se.SearchSource)
                .Where(se => se.IdentificationProcessId == processId)
                .ToListAsync(cancellationToken);
        }
    }
}
