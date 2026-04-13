using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.PaperEmbeddingRepo
{
    public interface IPaperEmbeddingRepository : IGenericRepository<PaperEmbedding, Guid, AppDbContext>
    {
        Task<PaperEmbedding?> FindClosestByCosineDistanceAsync(
            float[] embedding,
            Guid currentPaperId,
            CancellationToken cancellationToken = default,
            int take = 1);

        Task<List<PaperEmbedding>> FindClosestByCosineDistanceInIdentificationProcessAsync(
            float[] embedding,
            Guid identificationProcessId,
            CancellationToken cancellationToken = default,
            int take = 5);
    }
}
