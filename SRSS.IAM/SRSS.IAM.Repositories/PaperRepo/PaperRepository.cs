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

        public async Task<Paper?> GetByDoiAsync(string doi, CancellationToken cancellationToken = default)
        {
            return await _context.Papers
                .FirstOrDefaultAsync(p => p.DOI == doi, cancellationToken);
        }
    }
}

