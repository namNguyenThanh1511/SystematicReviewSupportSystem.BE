using Microsoft.EntityFrameworkCore;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.CandidatePaperRepo
{
    public class CandidatePaperRepository : GenericRepository<CandidatePaper, Guid, AppDbContext>, ICandidatePaperRepository
    {
        public CandidatePaperRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public IQueryable<CandidatePaper> GetCandidatesQueryable(Guid reviewProcessId)
        {
            return _context.CandidatePapers
                .Include(c => c.OriginPaper)
                .Where(c => c.ReviewProcessId == reviewProcessId)
                .AsNoTracking();
        }
    }
}
