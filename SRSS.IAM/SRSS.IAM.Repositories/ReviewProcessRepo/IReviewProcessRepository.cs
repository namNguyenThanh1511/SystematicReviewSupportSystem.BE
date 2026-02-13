using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.ReviewProcessRepo
{
    public interface IReviewProcessRepository : IGenericRepository<ReviewProcess, Guid, AppDbContext>
    {
        Task<ReviewProcess?> GetByIdWithProjectAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<ReviewProcess>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
        Task<ReviewProcess?> GetInProgressProcessForProjectAsync(Guid projectId, CancellationToken cancellationToken = default);
    }
}
