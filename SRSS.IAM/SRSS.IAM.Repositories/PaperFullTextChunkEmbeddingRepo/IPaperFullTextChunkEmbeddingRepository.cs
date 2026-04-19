using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.PaperFullTextChunkEmbeddingRepo
{
    public interface IPaperFullTextChunkEmbeddingRepository : IGenericRepository<PaperFullTextChunkEmbedding, Guid, AppDbContext>
    {
    }
}
