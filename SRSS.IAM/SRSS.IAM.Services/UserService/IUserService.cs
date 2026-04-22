using SRSS.IAM.Services.DTOs.Common;
using SRSS.IAM.Services.DTOs.User;

namespace SRSS.IAM.Services.UserService
{
    public interface IUserService
    {
        Task<IEnumerable<UserSearchResponse>> SearchUsersAsync(Guid projectId, string keyword, int limit = 15);
        Task<PaginatedResponse<UserResponse>> GetUsersAsync(UserListRequest request);
        Task<UserResponse> UpdateUserProfileAsync(Guid userId, UpdateUserProfileRequest request);
        Task<UserResponse> ToggleUserStatusAsync(Guid userId);
        Task<PaginatedResponse<UserProgressOverviewResponse>> GetUserProgressOverviewAsync(UserProgressRequest request);
    }
}
