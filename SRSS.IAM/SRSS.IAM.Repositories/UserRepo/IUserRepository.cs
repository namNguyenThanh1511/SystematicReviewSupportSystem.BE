using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;


namespace SRSS.IAM.Repositories.UserRepo
{
    public interface IUserRepository : IGenericRepository<User, Guid, AppDbContext>
    {
    }
}
