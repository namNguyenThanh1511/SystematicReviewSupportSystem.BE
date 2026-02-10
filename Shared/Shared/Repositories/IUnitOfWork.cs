using Microsoft.EntityFrameworkCore;

namespace Shared.Repositories
{
    public interface IUnitOfWork<TDbContext> : IDisposable
        where TDbContext : DbContext
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}