using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Shared.Entities.BaseEntity;
using SRSS.IAM.Repositories.IdentificationProcessRepo;
using SRSS.IAM.Repositories.ImportBatchRepo;
using SRSS.IAM.Repositories.PaperRepo;
using SRSS.IAM.Repositories.SearchExecutionRepo;
using SRSS.IAM.Repositories.UserRepo;
using SRSS.IAM.Repositories.SystematicReviewProjectRepo;
using SRSS.IAM.Repositories.ReviewProcessRepo;
using SRSS.IAM.Repositories.PrismaReportRepo;

namespace SRSS.IAM.Repositories.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _dbContext;
        private IDbContextTransaction? _currentTransaction;
        private IUserRepository _users;
        private ISystematicReviewProjectRepository _systematicReviewProjects;
        private IReviewProcessRepository _reviewProcesses;
        private IIdentificationProcessRepository _identificationProcesses;
        private ISearchExecutionRepository _searchExecutions;
        private IPaperRepository _papers;
        private IImportBatchRepository _importBatches;
        private IPrismaReportRepository _prismaReports;

        public UnitOfWork(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction != null) return;
            _currentTransaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null) return;

            try
            {
                await SaveChangesAsync(cancellationToken);
                await _currentTransaction.CommitAsync(cancellationToken);
            }
            finally
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null) return;

            try
            {
                await _currentTransaction.RollbackAsync(cancellationToken);
            }
            finally
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var date = DateTime.UtcNow;
            foreach (var entry in _dbContext.ChangeTracker.Entries<IBaseEntity>())
            {
                if (entry.State == EntityState.Added)
                    entry.Entity.CreatedAt = date;

                if (entry.State == EntityState.Added || entry.State == EntityState.Modified || entry.HasChangedOwnedEntities())
                    entry.Entity.ModifiedAt = date;
            }

            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public IUserRepository Users
            => _users ??= new UserRepository(_dbContext);

        public ISystematicReviewProjectRepository SystematicReviewProjects
            => _systematicReviewProjects ??= new SystematicReviewProjectRepository(_dbContext);

        public IReviewProcessRepository ReviewProcesses
            => _reviewProcesses ??= new ReviewProcessRepository(_dbContext);

        public IIdentificationProcessRepository IdentificationProcesses
            => _identificationProcesses ??= new IdentificationProcessRepository(_dbContext);

        public ISearchExecutionRepository SearchExecutions
            => _searchExecutions ??= new SearchExecutionRepository(_dbContext);

        public IPaperRepository Papers
            => _papers ??= new PaperRepository(_dbContext);

        public IImportBatchRepository ImportBatches 
            => _importBatches ??= new ImportBatchRepository(_dbContext);

        public IPrismaReportRepository PrismaReports 
            => _prismaReports ??= new PrismaReportRepository(_dbContext);

        public void Dispose() => _dbContext.Dispose();
    }
    public static class Extensions
    {
        public static bool HasChangedOwnedEntities(this EntityEntry entry) =>
            entry.References.Any(r =>
                r.TargetEntry != null &&
                r.TargetEntry.Metadata.IsOwned() &&
                (r.TargetEntry.State == EntityState.Added || r.TargetEntry.State == EntityState.Modified));
    }
}