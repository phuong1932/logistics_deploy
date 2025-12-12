using logistic_web.infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace logistic_web.infrastructure.Repositories
{
    public interface ITrackingRepository : IRepository<Tracking>
    {
        Task<IEnumerable<Tracking>> GetTrackingsByUserIdAsync(int userId);
    }

    public class TrackingRepository : Repository<Tracking>, ITrackingRepository
    {
        public TrackingRepository(LogisticContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Tracking>> GetTrackingsByUserIdAsync(int userId)
        {
            // Lấy username từ User table, sau đó lấy tracking theo username
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return new List<Tracking>();
            }

            return await _context.Trackings
                .Where(t => t.Username == user.Username)
                .OrderByDescending(t => t.DateCreated)
                .ToListAsync();
        }
    }
}

