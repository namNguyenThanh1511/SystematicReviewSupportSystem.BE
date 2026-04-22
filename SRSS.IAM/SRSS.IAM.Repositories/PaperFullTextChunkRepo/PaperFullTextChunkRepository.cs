using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.PaperFullTextChunkRepo
{
    public class PaperFullTextChunkRepository : GenericRepository<PaperFullTextChunk, Guid, AppDbContext>, IPaperFullTextChunkRepository
    {
        public PaperFullTextChunkRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
    }
}
