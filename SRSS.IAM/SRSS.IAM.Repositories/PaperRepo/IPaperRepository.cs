using Microsoft.EntityFrameworkCore;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.PaperRepo
{
    public interface IPaperRepository : IGenericRepository<Paper, Guid, AppDbContext>
    {
        Task<Paper?> GetByDoiAndProjectAsync(string doi, Guid projectId, CancellationToken cancellationToken = default);

        Task<(List<Paper> Papers, int TotalCount)> GetPapersByProjectAsync(
            Guid projectId,
            string? search,
            SelectionStatus? status,
            int? year,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);
    }
}
