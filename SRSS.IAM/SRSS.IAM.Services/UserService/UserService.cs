using Microsoft.EntityFrameworkCore;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.Common;
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

        public async Task<PaginatedResponse<UserResponse>> GetUsersAsync(UserListRequest request)
        {
            var (users, totalCount) = await _unitOfWork.Users.GetPaginatedUsersAsync(
                request.Search,
                request.IsActive,
                request.PageNumber,
                request.PageSize);

            return new PaginatedResponse<UserResponse>
            {
                Items = users.Select(u => u.ToUserResponse()).ToList(),
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<UserResponse> UpdateUserProfileAsync(Guid userId, UpdateUserProfileRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                throw new ArgumentException("Email is required.");
            if (string.IsNullOrWhiteSpace(request.Username))
                throw new ArgumentException("Username is required.");
            if (string.IsNullOrWhiteSpace(request.FullName))
                throw new ArgumentException("FullName is required.");

            var user = await _unitOfWork.Users.FindSingleAsync(u => u.Id == userId);
            if (user == null)
                throw new InvalidOperationException($"User with ID {userId} not found.");

            // Check if email is already taken by another user
            if (!string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase))
            {
                var emailExists = await _unitOfWork.Users.AnyAsync(u => u.Email.ToLower() == request.Email.ToLower());
                if (emailExists)
                    throw new InvalidOperationException($"Email '{request.Email}' is already taken.");
                user.Email = request.Email;
            }

            // Check if username is already taken by another user
            if (!string.Equals(user.Username, request.Username, StringComparison.OrdinalIgnoreCase))
            {
                var usernameExists = await _unitOfWork.Users.AnyAsync(u => u.Username.ToLower() == request.Username.ToLower());
                if (usernameExists)
                    throw new InvalidOperationException($"Username '{request.Username}' is already taken.");
                user.Username = request.Username;
            }

            user.FullName = request.FullName;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return user.ToUserResponse();
        }

        public async Task<UserResponse> ToggleUserStatusAsync(Guid userId)
        {
            var user = await _unitOfWork.Users.FindSingleAsync(u => u.Id == userId);
            if (user == null)
                throw new InvalidOperationException($"User with ID {userId} not found.");

            user.IsActive = !user.IsActive;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return user.ToUserResponse();
        }
    }
}
