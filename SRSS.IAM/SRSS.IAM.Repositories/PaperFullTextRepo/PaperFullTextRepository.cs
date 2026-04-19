using System;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.PaperFullTextRepo
{
    public class PaperFullTextRepository : GenericRepository<PaperFullText, Guid, AppDbContext>, IPaperFullTextRepository
    {
        public PaperFullTextRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<string?> GetRawXmlByPaperIdAsync(Guid paperId, CancellationToken cancellationToken)
        {
            return await _context.PaperFullTexts
                .Include(x => x.PaperPdf)
                .Where(x => x.PaperPdf.PaperId == paperId)
                .Select(x => x.RawXml)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
