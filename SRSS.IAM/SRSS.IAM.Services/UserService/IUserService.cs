using SRSS.IAM.Services.DTOs.User;

namespace SRSS.IAM.Services.UserService
{
    public interface IUserService
    {
        Task<IEnumerable<UserSearchResponse>> SearchUsersAsync(Guid projectId, string keyword, int limit = 15);
    }
}
