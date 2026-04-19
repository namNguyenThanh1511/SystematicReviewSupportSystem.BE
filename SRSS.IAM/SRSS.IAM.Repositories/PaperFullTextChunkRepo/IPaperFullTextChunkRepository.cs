using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.PaperFullTextChunkRepo
{
    public interface IPaperFullTextChunkRepository : IGenericRepository<PaperFullTextChunk, Guid, AppDbContext>
    {
    }
}
