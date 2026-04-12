using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.ExclusionReasonLibraryRepo
{
    public interface IExclusionReasonLibraryRepository : IGenericRepository<ExclusionReasonLibrary, Guid, AppDbContext>
    {
        Task<(List<ExclusionReasonLibrary> Items, int TotalCount)> GetPaginatedAsync(
            string? search,
            bool? onlyActive,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);
    }
}
