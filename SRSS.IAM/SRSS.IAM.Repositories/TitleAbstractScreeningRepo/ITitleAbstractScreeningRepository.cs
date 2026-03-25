using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.TitleAbstractScreeningRepo
{
    public interface ITitleAbstractScreeningRepository : IGenericRepository<TitleAbstractScreening, Guid, AppDbContext>
    {
        Task<TitleAbstractScreening?> GetByProcessIdAsync(
            Guid studySelectionProcessId,
            CancellationToken cancellationToken = default);
    }
}
