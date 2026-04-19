using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.PaperFullTextChunkEmbeddingRepo
{
    public class PaperFullTextChunkEmbeddingRepository : GenericRepository<PaperFullTextChunkEmbedding, Guid, AppDbContext>, IPaperFullTextChunkEmbeddingRepository
    {
        public PaperFullTextChunkEmbeddingRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
    }
}
