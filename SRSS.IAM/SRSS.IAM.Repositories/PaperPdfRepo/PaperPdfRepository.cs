using System;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.PaperPdfRepo
{
    public class PaperPdfRepository : GenericRepository<PaperPdf, Guid, AppDbContext>, IPaperPdfRepository
    {
        public PaperPdfRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<PaperPdf?> GetLatestPaperPdfAsync(Guid paperId, CancellationToken cancellationToken = default)
        {
            return await _context.PaperPdfs
                .Where(p => p.PaperId == paperId)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
