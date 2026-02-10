using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Services.DTOs.User;

namespace SRSS.IAM.Services.Mappers
{
    public static class UserMappingExtension
    {
        public static UserResponse ToUserResponse(User user)
        {
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
        public static UserProfileResponse ToUserProfileResponse(this User user)
        {
            return new UserProfileResponse
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                FullName = user.FullName,
                Role = user.Role.ToString()
            };
        }



    }
}
