using SRSS.IAM.Repositories.UserRepo;

namespace SRSS.IAM.Repositories.UnitOfWork
{
    public class UnitOfWork : Shared.Repositories.UnitOfWork<AppDbContext>, IUnitOfWork
    {

        private IUserRepository _users;

        public UnitOfWork(AppDbContext dbContext) : base(dbContext)
        {
            //_dbContext được quản lí bởi lớp cha
        }

        public IUserRepository Users
       => _users ??= new UserRepository(_dbContext);
    }
}