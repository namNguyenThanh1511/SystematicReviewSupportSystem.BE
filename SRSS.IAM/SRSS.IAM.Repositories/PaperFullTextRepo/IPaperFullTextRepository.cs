using System;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.PaperFullTextRepo
{
    public interface IPaperFullTextRepository : IGenericRepository<PaperFullText, Guid, AppDbContext>
    {
        Task<string?> GetRawXmlByPaperIdAsync(Guid paperId, CancellationToken cancellationToken);
    }
}
