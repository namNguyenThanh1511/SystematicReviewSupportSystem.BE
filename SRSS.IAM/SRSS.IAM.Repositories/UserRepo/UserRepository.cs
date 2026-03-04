using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UserRepo.DTOs;

namespace SRSS.IAM.Repositories.UserRepo
{
    public class UserRepository : GenericRepository<User, Guid, AppDbContext>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<UserSearchResultDto>> SearchUsersAsync(Guid projectId, string keyword, int limit = 15)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.IsActive &&
                           (u.Email.StartsWith(keyword) ||
                            u.Username.StartsWith(keyword)))
                .OrderBy(u => u.Username)
                .Select(u => new UserSearchResultDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    Username = u.Username,
                    FullName = u.FullName,
                    ProjectRole = _context.ProjectMembers
                        .Where(pm => pm.ProjectId == projectId && pm.UserId == u.Id)
                        .Select(pm => pm.Role)
                        .FirstOrDefault(),
                    IsAlreadyMember = _context.ProjectMembers
                        .Any(pm => pm.ProjectId == projectId && pm.UserId == u.Id)
                })
                .Take(limit)
                .ToListAsync();
        }
    }
}