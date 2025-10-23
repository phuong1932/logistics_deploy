using logistic_web.infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace logistic_web.infrastructure.Repositories
{
    public interface ICargolistRepository : IRepository<Cargolist>
    {
      
        
        // Lấy thống kê tổng hợp: số lượng đơn hàng + tổng doanh thu (không cần truyền tham số)
        Task<(int count, decimal totalRevenue)> GetMonthlyStatisticsAsync();
    }

    public class CargolistRepository : Repository<Cargolist>, ICargolistRepository
    {
        public CargolistRepository(LogisticContext context) : base(context)
        {
        }

        /// <summary>
        /// Lấy thống kê tổng hợp trong tháng hiện tại (tự động)
        /// Bao gồm: Số lượng đơn hàng + Tổng doanh thu
        /// Ví dụ: Nếu hiện tại là tháng 10/2025 thì tính từ 1/10/2025 đến 31/10/2025
        /// </summary>
        /// <returns>Tuple chứa (số lượng đơn hàng, tổng doanh thu)</returns>
        public async Task<(int count, decimal totalRevenue)> GetMonthlyStatisticsAsync()
        {
            var now = DateTime.Now;
            
            // Ngày đầu tháng hiện tại
            var startDate = new DateTime(now.Year, now.Month, 1);
            // Ngày cuối tháng hiện tại
            var endDate = startDate.AddMonths(1).AddDays(-1).Date.AddHours(23).AddMinutes(59).AddSeconds(59);
            
            // Lấy danh sách đơn hàng trong tháng
            var cargosInMonth = await _context.Set<Cargolist>()
                .Where(c => c.CreatedAt.HasValue && 
                           c.CreatedAt.Value >= startDate && 
                           c.CreatedAt.Value <= endDate)
                .ToListAsync();
            
            // Tính số lượng và tổng doanh thu
            var count = cargosInMonth.Count;
            var totalRevenue = cargosInMonth.Sum(c => c.EstimatedTotalAmount ?? 0);
            
            return (count, totalRevenue);
        }
    }
}
