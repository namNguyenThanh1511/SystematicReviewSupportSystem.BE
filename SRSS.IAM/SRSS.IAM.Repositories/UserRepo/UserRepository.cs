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

        public async Task<(IEnumerable<User> Items, int TotalCount)> GetPaginatedUsersAsync(
            string? search,
            bool? isActive,
            int pageNumber,
            int pageSize)
        {
            var query = GetUsersQuery(search, isActive);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(u => u.Username)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        private IQueryable<User> GetUsersQuery(string? search, bool? isActive)
        {
            var query = _context.Users.AsNoTracking();

            // Search by Email, FullName, or Username
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.Trim().ToLower();
                query = query.Where(u =>
                    u.Username.ToLower().Contains(searchLower) ||
                    u.Email.ToLower().Contains(searchLower) ||
                    u.FullName.ToLower().Contains(searchLower));
            }

            if (isActive.HasValue)
            {
                query = query.Where(u => u.IsActive == isActive.Value);
            }

            return query;
        }
    }
}