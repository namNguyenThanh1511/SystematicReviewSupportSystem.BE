using System;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;

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

        public async Task<bool> AnyFullTextProcessedByHashAsync(string fileHash, CancellationToken cancellationToken = default)
        {
            return await _context.PaperPdfs
                .AnyAsync(p => p.FileHash == fileHash && (p.FullTextProcessed || p.ProcessingStatus == PdfProcessingStatus.Completed), cancellationToken);
        }
    }
}
