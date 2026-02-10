using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
    public class User : BaseEntity<Guid>
    {
        public string Username { get; set; }
        public string? Password { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public Role Role { get; set; }
        public bool IsActive { get; set; } = true;
        public string? RefreshToken { get; set; }
        public bool IsRefreshTokenRevoked { get; set; } = false;
        public DateTimeOffset? RefreshTokenExpiryTime { get; set; }
    }

    public enum Role
    {
        Client = 0,
        Admin = 1,
    }
}
