using logistic_web.infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace logistic_web.infrastructure.Repositories
{
    public interface IUserRoleRepository : IRepository<UserRole>
    {
        Task<IEnumerable<UserRole>> GetByUserIdAsync(int userId);
        Task<IEnumerable<UserRole>> GetByRoleIdAsync(int roleId);
        Task<UserRole?> GetByUserAndRoleAsync(int userId, int roleId);
        Task<bool> ExistsAsync(int userId, int roleId);
        Task<bool> AssignRoleToUserAsync(int userId, int roleId, string? assignedBy = null);
        Task<bool> RemoveRoleFromUserAsync(int userId, int roleId);
        Task<IEnumerable<User>> GetUsersByRoleAsync(int roleId);
        Task<IEnumerable<Role>> GetRolesByUserAsync(int userId);
    }

    public class UserRoleRepository : Repository<UserRole>, IUserRoleRepository
    {
        public UserRoleRepository(LogisticContext context) : base(context)
        {
        }

        public async Task<IEnumerable<UserRole>> GetByUserIdAsync(int userId)
        {
            return await _context.UserRoles
                .Include(ur => ur.User)
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserRole>> GetByRoleIdAsync(int roleId)
        {
            return await _context.UserRoles
                .Include(ur => ur.User)
                .Include(ur => ur.Role)
                .Where(ur => ur.RoleId == roleId)
                .ToListAsync();
        }

        public async Task<UserRole?> GetByUserAndRoleAsync(int userId, int roleId)
        {
            return await _context.UserRoles
                .Include(ur => ur.User)
                .Include(ur => ur.Role)
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
        }

        public async Task<bool> ExistsAsync(int userId, int roleId)
        {
            return await _context.UserRoles
                .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
        }

        public async Task<bool> AssignRoleToUserAsync(int userId, int roleId, string? assignedBy = null)
        {
            if (await ExistsAsync(userId, roleId))
                return false;

            var userRole = new UserRole
            {
                UserId = userId,
                RoleId = roleId,
                Description = assignedBy
            };

            await _context.UserRoles.AddAsync(userRole);
            return true;
        }

        public async Task<bool> RemoveRoleFromUserAsync(int userId, int roleId)
        {
            var userRole = await GetByUserAndRoleAsync(userId, roleId);
            if (userRole == null)
                return false;

            _context.UserRoles.Remove(userRole);
            return true;
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(int roleId)
        {
            return await _context.UserRoles
                .Include(ur => ur.User)
                .Where(ur => ur.RoleId == roleId)
                .Select(ur => ur.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<Role>> GetRolesByUserAsync(int userId)
        {
            return await _context.UserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.Role)
                .ToListAsync();
        }
    }
}
