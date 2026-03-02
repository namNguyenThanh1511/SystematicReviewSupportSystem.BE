using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
using SRSS.IAM.Services.DTOs.User;
using SRSS.IAM.Services.UserService;

namespace SRSS.IAM.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UserController : BaseController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
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
    }
}
