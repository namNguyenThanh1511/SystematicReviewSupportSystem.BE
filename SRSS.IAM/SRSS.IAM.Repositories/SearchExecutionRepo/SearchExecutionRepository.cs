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
                .FirstOrDefaultAsync(se => se.Id == id, cancellationToken);
        }
    }
}
