using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Services.DTOs.User
{
    public class UserSearchResponse
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public ProjectRole? ProjectRole { get; set; }
        public bool IsAlreadyMember { get; set; }
    }
}
