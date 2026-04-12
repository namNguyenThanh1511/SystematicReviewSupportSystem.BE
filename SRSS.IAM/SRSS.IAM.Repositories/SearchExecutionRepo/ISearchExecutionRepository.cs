using Microsoft.EntityFrameworkCore;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.SearchExecutionRepo
{
    public interface ISearchExecutionRepository : IGenericRepository<SearchExecution, Guid, AppDbContext>
    {
        Task<SearchExecution?> GetByIdWithProjectAsync(Guid id, CancellationToken cancellationToken = default);
        Task<SearchExecution?> GetByIdWithSourceAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<SearchExecution>> GetByProcessIdWithSourceAsync(Guid processId, CancellationToken cancellationToken = default);
    }
}
