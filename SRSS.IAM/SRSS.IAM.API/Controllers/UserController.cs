using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
using SRSS.IAM.Services.DTOs.Common;
using SRSS.IAM.Services.DTOs.User;
using SRSS.IAM.Services.UserService;
using SRSS.IAM.Services.UserExportService;

namespace SRSS.IAM.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UserController : BaseController
    {
        private readonly IUserService _userService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUserExportService _userExportService;

        public UserController(IUserService userService, ICurrentUserService currentUserService, IUserExportService userExportService)
        {
            _userService = userService;
            _currentUserService = currentUserService;
            _userExportService = userExportService;
        }

        /// <summary>
        /// Get the profile of the currently authenticated user.
        /// </summary>
        /// <returns>Profile information of the current user</returns>
        [HttpGet("me")]
        public async Task<ActionResult<ApiResponse<UserResponse>>> GetProfile()
        {
            var userId = Guid.Parse(_currentUserService.GetUserId());
            var result = await _userService.GetUserByIdAsync(userId);
            return Ok(result, "Profile retrieved successfully.");
        }

        /// <summary>
        /// Change the password of the currently authenticated user.
        /// </summary>
        /// <param name="request">Old and new password information</param>
        /// <returns>Success message</returns>
        [HttpPost("change-password")]
        public async Task<ActionResult<ApiResponse>> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = Guid.Parse(_currentUserService.GetUserId());
            await _userService.ChangePasswordAsync(userId, request);
            return Ok("Password changed successfully.");
        }

        /// <summary>
        /// Search for users by keyword (Email or Username) to invite to a project.
        /// Matches if keyword is at least 3 characters.
        /// </summary>
        /// <param name="projectId">ID of the project for awareness</param>
        /// <param name="keyword">Keyword to search for (min 3 chars)</param>
        /// <returns>List of users matching the keyword with project membership info</returns>
        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse<IEnumerable<UserSearchResponse>>>> SearchUsers(
            [FromQuery] Guid projectId,
            [FromQuery] string keyword)
        {
            var result = await _userService.SearchUsersAsync(projectId, keyword);
            return Ok(result, "Users retrieved successfully.");
        }

        /// <summary>
        /// Get a paginated list of users with optional filtering and searching.
        /// </summary>
        /// <param name="request">Filter, search and pagination parameters</param>
        /// <returns>Paginated list of users</returns>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<UserResponse>>>> GetUsers([FromQuery] UserListRequest request)
        {
            var result = await _userService.GetUsersAsync(request);
            return Ok(result, "Users retrieved successfully.");
        }

        /// <summary>
        /// Export all user accounts to an Excel file.
        /// </summary>
        /// <param name="request">Export parameters</param>
        /// <returns>Excel file (.xlsx)</returns>
        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportToExcel([FromQuery] UserExportRequest request)
        {
            var fileBytes = await _userExportService.ExportUsersToExcelAsync(request);
            var fileName = $"user-accounts-export-{DateTimeOffset.UtcNow:yyyy-MM-dd}.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        /// <summary>
        /// Update a user's profile information.
        /// Only allows updating FullName, Email, and Username.
        /// </summary>
        /// <param name="userId">ID of the user to update</param>
        /// <param name="request">Updated profile information</param>
        /// <returns>Updated user information</returns>
        [HttpPut("{userId}/profile")]
        public async Task<ActionResult<ApiResponse<UserResponse>>> UpdateProfile([FromRoute] Guid userId, [FromBody] UpdateUserProfileRequest request)
        {
            var result = await _userService.UpdateUserProfileAsync(userId, request);
            return Ok(result, "Profile updated successfully.");
        }

        /// <summary>
        /// Toggle the active status of a user account.
        /// </summary>
        /// <param name="userId">ID of the user to toggle</param>
        /// <returns>Updated user information</returns>
        [HttpPatch("{userId}/status")]
        public async Task<ActionResult<ApiResponse<UserResponse>>> ToggleStatus([FromRoute] Guid userId)
        {
            var result = await _userService.ToggleUserStatusAsync(userId);
            var statusMessage = result.IsActive ? "Account activated successfully." : "Account deactivated successfully.";
            return Ok(result, statusMessage);
        }

        /// <summary>
        /// Get user progress overview for a project.
        /// </summary>
        /// <param name="request">Project ID, search and pagination parameters</param>
        /// <returns>Paginated list of user progress overview</returns>
        [HttpGet("progress-overview")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<UserProgressOverviewResponse>>>> GetUserProgressOverview([FromQuery] UserProgressRequest request)
        {
            var result = await _userService.GetUserProgressOverviewAsync(request);
            return Ok(result, "User progress overview retrieved successfully.");
        }
    }
}
