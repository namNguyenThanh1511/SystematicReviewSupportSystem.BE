using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Services.JWTService
{
    public interface IJwtService
    {
        /// <summary>
        /// Tạo JWT access token
        /// </summary>
        string GenerateAccessToken(User user);

        /// <summary>
        /// Validate access token
        /// </summary>
        bool ValidateAccessToken(string token);

    }
}
