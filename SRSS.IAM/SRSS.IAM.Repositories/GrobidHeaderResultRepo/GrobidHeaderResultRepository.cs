using System;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.GrobidHeaderResultRepo
{
    public class GrobidHeaderResultRepository : GenericRepository<GrobidHeaderResult, Guid, AppDbContext>, IGrobidHeaderResultRepository
    {
        public GrobidHeaderResultRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<GrobidHeaderResult?> GetLatestGrobidHeaderResultAsync(Guid paperId, CancellationToken cancellationToken = default)
        {
            return await _context.GrobidHeaderResults.OrderByDescending(ghr => ghr.CreatedAt).FirstOrDefaultAsync(ghr => ghr.PaperPdfId == paperId, cancellationToken);
        }
        
    }
}
