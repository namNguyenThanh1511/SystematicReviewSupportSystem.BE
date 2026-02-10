using Shared.Repositories;
using SRSS.IAM.Repositories.UserRepo;

namespace SRSS.IAM.Repositories.UnitOfWork
{
    // Service-specific interface extends base
    public interface IUnitOfWork : IUnitOfWork<AppDbContext>
    {
        IUserRepository Users { get; }
    }
}