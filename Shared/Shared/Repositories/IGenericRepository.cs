using Microsoft.EntityFrameworkCore;
using Shared.Entities.BaseEntity;
using System.Linq.Expressions;

namespace Shared.Repositories
{
    public interface IGenericRepository<TEntity, in TKey, TDbContext>
        where TEntity : BaseEntity<TKey>
        where TDbContext : DbContext
    {
        Task<IEnumerable<TEntity>> FindAllAsync(
            Expression<Func<TEntity, bool>>? predicate = null,
            bool isTracking = true,
            CancellationToken cancellationToken = default);

        Task<TEntity?> FindSingleAsync(
            Expression<Func<TEntity, bool>>? predicate = null,
            bool isTracking = true,
            CancellationToken cancellationToken = default);

        Task<bool> AnyAsync(
            Expression<Func<TEntity, bool>>? predicate = null,
            bool isTracking = true,
            CancellationToken cancellationToken = default);

        Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
        Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task RemoveMultipleAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    }
}