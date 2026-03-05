using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UserRepo.DTOs;


namespace SRSS.IAM.Repositories.UserRepo
{
    public interface IUserRepository : IGenericRepository<User, Guid, AppDbContext>
    {
        Task<IEnumerable<UserSearchResultDto>> SearchUsersAsync(Guid projectId, string keyword, int limit = 15);
    }
}
