using Microsoft.EntityFrameworkCore;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.User;
using SRSS.IAM.Services.Mappers;

namespace SRSS.IAM.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<UserSearchResponse>> SearchUsersAsync(Guid projectId, string keyword, int limit = 15)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return Enumerable.Empty<UserSearchResponse>();
            }

            var trimmedKeyword = keyword.Trim();
            if (trimmedKeyword.Length < 3)
            {
                throw new ArgumentException("Search keyword must be at least 3 characters long.");
            }

            var users = await _unitOfWork.Users.SearchUsersAsync(projectId, trimmedKeyword, limit);

            return users.Select(u => new UserSearchResponse
            {
                Id = u.Id,
                Email = u.Email,
                Username = u.Username,
                FullName = u.FullName,
                ProjectRole = u.ProjectRole,
                IsAlreadyMember = u.IsAlreadyMember
            });
        }
    }
}
