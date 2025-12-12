using logistic_web.infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace logistic_web.infrastructure.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
        Task<bool> ExistsByUsernameAsync(string username);
        Task<bool> ExistsByEmailAsync(string email);
        Task<IEnumerable<User>> GetUsersByRoleAsync(int roleId);
    }

    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(LogisticContext context) : base(context)
        {
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(u => u.UserRole)
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.UserRole)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> ExistsByUsernameAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.Username == username);
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(int roleId)
        {
            return await _context.Users
                .Include(u => u.UserRole)
                .Where(u => u.UserRole != null && u.UserRole.RoleId == roleId && u.Deleted != true)
                .ToListAsync();
        }
        
    }
}
