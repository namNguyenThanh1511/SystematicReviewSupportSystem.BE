using SRSS.IAM.Services.DTOs.User;

namespace SRSS.IAM.Services.UserService
{
    public interface IUserService
    {
        Task<UserResponse?> GetUserByEmailAsync(string email);
    }
}
