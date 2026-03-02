using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.UserRepo.DTOs
{
    public class UserSearchResultDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public ProjectRole? ProjectRole { get; set; }
    }
}
