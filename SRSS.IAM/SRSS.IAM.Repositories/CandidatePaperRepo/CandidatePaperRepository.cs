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

        public async Task<List<CandidatePaper>> GetCandidatePapersByPaperId(Guid paperId, CancellationToken ct = default)
        {

            var candidatePapers = await _context.CandidatePapers.Where(c => c.OriginPaperId == paperId)
            .Include(c => c.OriginPaper)
            .ToListAsync(ct);
            return candidatePapers;
        }

        public IQueryable<CandidatePaper> GetCandidatesQueryable()
        {
            return _context.CandidatePapers
                .Include(c => c.OriginPaper)
                .AsNoTracking();
        }
    }
}
