using logistic_web.infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace logistic_web.infrastructure.Repositories
{
    public interface IRoleRepository : IRepository<Role>
    {
        Task<Role?> GetByNameAsync(string roleName);
        Task<bool> ExistsByNameAsync(string roleName);
        Task<IEnumerable<Role>> GetRolesByUserIdAsync(int userId);
        Task<string?> GetRoleNameByIdAsync(int roleId);
    }

    public class RoleRepository : Repository<Role>, IRoleRepository
    {
        public RoleRepository(LogisticContext context) : base(context)
        {
        }

        public async Task<Role?> GetByNameAsync(string roleName)
        {
            return await _context.Roles
                .Include(r => r.UserRoles)
                .FirstOrDefaultAsync(r => r.RoleName == roleName);
        }

        public async Task<bool> ExistsByNameAsync(string roleName)
        {
            return await _context.Roles.AnyAsync(r => r.RoleName == roleName);
        }

        public async Task<IEnumerable<Role>> GetRolesByUserIdAsync(int userId)
        {
            return await _context.Roles
                .Include(r => r.UserRoles)
                .Where(r => r.UserRoles.Any(ur => ur.UserId == userId) && r.Deleted != true)
                .ToListAsync();
        }

        public async Task<string?> GetRoleNameByIdAsync(int roleId)
        {
            var rolename = await _context.Roles
                .Where(r => r.Id == roleId && r.Deleted != true)
                .Select(r => r.RoleName)
                .FirstOrDefaultAsync();

            return rolename;
        }
        
    }
}
