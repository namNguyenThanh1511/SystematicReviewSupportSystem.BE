using Microsoft.EntityFrameworkCore;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.User;

namespace SRSS.IAM.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<UserResponse?> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            var normalizedEmail = email.Trim().ToLower();
            var user = await _unitOfWork.Users.GetByEmailAsync(normalizedEmail);

            if (user == null)
            {
                return null;
            }

            return new UserResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Username = user.Username,
                IsActive = user.IsActive,
                Role = user.Role.ToString()
            };
        }
    }
}
