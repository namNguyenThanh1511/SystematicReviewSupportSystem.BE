using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.SystematicReviewProjectRepo
{
    public interface ISystematicReviewProjectRepository : IGenericRepository<SystematicReviewProject, Guid, AppDbContext>
    {
        Task<SystematicReviewProject?> GetByIdWithProcessesAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<SystematicReviewProject>> GetActiveProjectsAsync(CancellationToken cancellationToken = default);
        IQueryable<SystematicReviewProject> GetQueryable();
    }
}
