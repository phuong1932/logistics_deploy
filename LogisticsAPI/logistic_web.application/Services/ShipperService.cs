using logistic_web.infrastructure.Models;
using logistic_web.infrastructure.Repositories;
using logistic_web.infrastructure.Unitofwork;
using logistic_web.application.DTO;
using Microsoft.Extensions.Logging;

namespace logistic_web.application.Services
{
    public interface IShipperService
    {
        Task<IEnumerable<ShipperResponse>> GetAllShippersAsync();
        Task<ShipperResponse?> GetShipperByIdAsync(int id);
        Task<Shipper> CreateShipperAsync(ShipperDto shipperDto);
        Task<Shipper?> UpdateShipperAsync(int id, ShipperDto shipperDto);
        Task<bool> DeleteShipperAsync(int id);
        Task<IEnumerable<ShipperResponse>> SearchShipperByNameAsync(string name);
        Task<string?> GetShipperNameByIdAsync(int id);
    }

    public class ShipperService : IShipperService
    {
        private readonly IShipperRepository _shipperRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ShipperService> _logger;

        public ShipperService(
            IShipperRepository shipperRepository,
            IUnitOfWork unitOfWork,
            ILogger<ShipperService> logger)
        {
            _shipperRepository = shipperRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<ShipperResponse>> GetAllShippersAsync()
        {
            try
            {
                var shippers = await _shipperRepository.GetAllAsync();
                _logger.LogInformation("Lấy danh sách {Count} shipper", shippers.Count());
                return shippers.Select(s => new ShipperResponse
                {
                    Id = s.Id,
                    TenTaiXe = s.TenTaiXe,
                    LoaiXe = s.LoaiXe,
                    SoDienThoai = s.SoDienThoai,
                    DiaChi = s.DiaChi
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách shipper");
                throw;
            }
        }

        public async Task<ShipperResponse?> GetShipperByIdAsync(int id)
        {
            try
            {
                var shipper = await _shipperRepository.GetByIdAsync(id);
                if (shipper == null)
                {
                    return null;
                }

                _logger.LogInformation("Lấy thông tin shipper ID: {ShipperId}", id);
                return new ShipperResponse
                {
                    Id = shipper.Id,
                    TenTaiXe = shipper.TenTaiXe,
                    LoaiXe = shipper.LoaiXe,
                    SoDienThoai = shipper.SoDienThoai,
                    DiaChi = shipper.DiaChi
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy shipper ID: {ShipperId}", id);
                throw;
            }
        }

        public async Task<Shipper> CreateShipperAsync(ShipperDto shipperDto)
        {
            try
            {
                var shipper = new Shipper
                {
                    TenTaiXe = shipperDto.TenTaiXe,
                    LoaiXe = shipperDto.LoaiXe,
                    SoDienThoai = shipperDto.SoDienThoai,
                    DiaChi = shipperDto.DiaChi
                };

                await _shipperRepository.AddAsync(shipper);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Tạo shipper thành công: {TenTaiXe}", shipper.TenTaiXe);
                return shipper;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo shipper");
                throw;
            }
        }

        public async Task<Shipper?> UpdateShipperAsync(int id, ShipperDto shipperDto)
        {
            try
            {
                var shipper = await _shipperRepository.GetByIdAsync(id);
                if (shipper == null)
                {
                    _logger.LogWarning("Không tìm thấy shipper ID: {ShipperId}", id);
                    return null;
                }

                shipper.TenTaiXe = shipperDto.TenTaiXe;
                shipper.LoaiXe = shipperDto.LoaiXe;
                shipper.SoDienThoai = shipperDto.SoDienThoai;
                shipper.DiaChi = shipperDto.DiaChi;

                _shipperRepository.Update(shipper);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Cập nhật shipper thành công ID: {ShipperId}", id);
                return shipper;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật shipper ID: {ShipperId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteShipperAsync(int id)
        {
            try
            {
                var shipper = await _shipperRepository.GetByIdAsync(id);
                if (shipper == null)
                {
                    _logger.LogWarning("Không tìm thấy shipper để xóa ID: {ShipperId}", id);
                    return false;
                }

                _shipperRepository.Remove(shipper);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Xóa shipper thành công ID: {ShipperId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa shipper ID: {ShipperId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<ShipperResponse>> SearchShipperByNameAsync(string name)
        {
            try
            {
                var shippers = await _shipperRepository.FindAsync(
                    s => s.TenTaiXe != null && s.TenTaiXe.Contains(name)
                );
                _logger.LogInformation("Tìm kiếm shipper theo tên: {Name}, tìm thấy {Count} kết quả", name, shippers.Count());
                return shippers.Select(s => new ShipperResponse
                {
                    Id = s.Id,
                    TenTaiXe = s.TenTaiXe,
                    LoaiXe = s.LoaiXe,
                    SoDienThoai = s.SoDienThoai,
                    DiaChi = s.DiaChi
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm shipper theo tên: {Name}", name);
                throw;
            }
        }

        public async Task<string?> GetShipperNameByIdAsync(int id)
        {
            try
            {
                var shipper = await _shipperRepository.GetByIdAsync(id);
                if (shipper == null)
                {
                    _logger.LogWarning("Không tìm thấy shipper với ID: {ShipperId}", id);

                    return null;
                }
                return shipper.TenTaiXe;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy tên shipper theo ID: {ShipperId}", id);
                throw;
            }
        }
    }
}

