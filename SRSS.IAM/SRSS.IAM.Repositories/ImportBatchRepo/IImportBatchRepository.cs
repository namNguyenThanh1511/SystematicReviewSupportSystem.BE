using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.ImportBatchRepo
{
    public interface IImportBatchRepository : IGenericRepository<ImportBatch, Guid, AppDbContext>
    {
        Task<IEnumerable<ImportBatch>> GetByProjectIdsAsync(Guid projectId, CancellationToken cancellationToken = default);
    }
}
