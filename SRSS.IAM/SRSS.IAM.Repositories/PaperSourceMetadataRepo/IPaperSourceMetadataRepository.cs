using System;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.PaperSourceMetadataRepo
{
    public interface IPaperSourceMetadataRepository : IGenericRepository<PaperSourceMetadata, Guid, AppDbContext>
    {
        Task<PaperSourceMetadata?> GetLatestWithGrobidHeaderByPaperIdAsync(
            Guid paperId,
            CancellationToken cancellationToken = default);
    }
}
