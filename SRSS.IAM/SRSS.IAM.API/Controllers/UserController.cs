using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
using SRSS.IAM.Services.DTOs.User;
using SRSS.IAM.Services.UserService;

namespace SRSS.IAM.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : BaseController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Get user by email address
        /// </summary>
        /// <param name="email">Email address to search for</param>
        /// <returns>User details if found, otherwise 404</returns>
        [HttpGet("email/{email}")]
        public async Task<ActionResult<ApiResponse<UserResponse>>> GetByEmail(string email)
        {
            var result = await _userService.GetUserByEmailAsync(email);

            if (result == null)
            {
                // This will be caught by the client based on our conventions, 
                // but the prompt asked for 200 OK -> UserResponse or 404 NotFound.
                // However, our BaseController and GlobalExceptionMiddleware usually handle this.
                // Following the "Let exceptions bubble up" rule from user_rules:
                throw new InvalidOperationException($"User with email {email} not found.");
            }

            return Ok(result, "User retrieved successfully.");
        }
    }
}
