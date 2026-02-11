using Microsoft.EntityFrameworkCore;
using Shared.Repositories;
using SRSS.IAM.Repositories.UserRepo;
using SRSS.IAM.Repositories.IdentificationProcessRepo;
using SRSS.IAM.Repositories.SearchExecutionRepo;
using SRSS.IAM.Repositories.PaperRepo;
using SRSS.IAM.Repositories.ImportBatchRepo;
using SRSS.IAM.Repositories.SystematicReviewProjectRepo;
using SRSS.IAM.Repositories.ReviewProcessRepo;

namespace SRSS.IAM.Repositories.UnitOfWork
{
    // Service-specific interface extends base
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
        IUserRepository Users { get; }
        ISystematicReviewProjectRepository SystematicReviewProjects { get; }
        IReviewProcessRepository ReviewProcesses { get; }
        IIdentificationProcessRepository IdentificationProcesses { get; }
        ISearchExecutionRepository SearchExecutions { get; }
        IPaperRepository Papers { get; }

        IImportBatchRepository ImportBatches { get; }

        }
}