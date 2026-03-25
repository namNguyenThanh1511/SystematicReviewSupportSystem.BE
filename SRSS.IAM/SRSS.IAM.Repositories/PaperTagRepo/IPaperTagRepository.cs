using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.PaperTagRepo
{
    public interface IPaperTagRepository : IGenericRepository<PaperTag, Guid, AppDbContext>
    {
        Task<List<PaperTag>> GetTagsByPaperAsync(Guid paperId, CancellationToken cancellationToken = default);

        Task<List<PaperTag>> GetTagsByPaperAndPhaseAsync(Guid paperId, ProcessPhase phase, CancellationToken cancellationToken = default);

        Task<PaperTag?> GetExistingTagAsync(Guid paperId, Guid userId, ProcessPhase phase, string label, CancellationToken cancellationToken = default);
    }
}
