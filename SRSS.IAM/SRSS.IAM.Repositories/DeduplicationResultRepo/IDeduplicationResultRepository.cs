using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.DeduplicationResultRepo
{
    public interface IDeduplicationResultRepository : IGenericRepository<DeduplicationResult, Guid, AppDbContext>
    {
        Task<List<DeduplicationResult>> GetByIdentificationProcessAsync(
            Guid identificationProcessId, 
            CancellationToken cancellationToken = default);

        Task<DeduplicationResult?> GetByPaperAndProcessAsync(
            Guid paperId, 
            Guid identificationProcessId,
            CancellationToken cancellationToken = default);

        Task<int> CountDuplicatesByProcessAsync(
            Guid identificationProcessId,
            CancellationToken cancellationToken = default);
    }
}
