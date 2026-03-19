using System;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;

namespace SRSS.IAM.Repositories.PaperSourceMetadataRepo
{
    public class PaperSourceMetadataRepository : GenericRepository<PaperSourceMetadata, Guid, AppDbContext>, IPaperSourceMetadataRepository
    {
        public PaperSourceMetadataRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<PaperSourceMetadata?> GetLatestWithGrobidHeaderByPaperIdAsync(
            Guid paperId,
            CancellationToken cancellationToken = default)
        {
            return await _context.PaperSourceMetadatas
                .Where(s => s.PaperId == paperId && s.Source == MetadataSource.GROBID_HEADER)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
