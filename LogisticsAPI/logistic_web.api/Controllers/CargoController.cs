using Microsoft.AspNetCore.Mvc;
using logistic_web.application.Services;
using logistic_web.application.DTO;
using logistic_web.api.DTO;

namespace logistic_web.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CargoController : ControllerBase
    {
        private readonly ILogger<CargoController> _logger;
        private readonly ICargoService _cargoService;

        public CargoController(ILogger<CargoController> logger, ICargoService cargoService)
        {
            _logger = logger;
            _cargoService = cargoService;
        }

        /// <summary>
        /// Lấy danh sách tất cả cargo
        /// </summary>
        /// <returns>Danh sách cargo</returns>
        [HttpGet("getalllistcargo")]
        public async Task<ActionResult<object>> GetAllListCargo()
        {
            try
            {
                var cargos = await _cargoService.GetAllCargoAsync();
                return Ok(new { success = true, data = cargos, message = "Lấy danh sách cargo thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách cargo");
                return StatusCode(500, new { success = false, message = "Lỗi server khi lấy danh sách cargo" });
            }
        }

        /// <summary>
        /// Lấy cargo theo ID
        /// </summary>
        /// <param name="id">ID của cargo</param>
        /// <returns>Thông tin cargo</returns>
        [HttpGet("getcargobyid/{id}")]
        public async Task<ActionResult<object>> GetCargoById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { success = false, message = "ID cargo không hợp lệ" });
                }

                var cargo = await _cargoService.GetCargoByIdAsync(id);
                if (cargo == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy cargo" });
                }

                return Ok(new { success = true, data = cargo, message = "Lấy thông tin cargo thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy cargo ID: {CargoId}", id);
                return StatusCode(500, new { success = false, message = "Lỗi server khi lấy thông tin cargo" });
            }
        }

        /// <summary>
        /// Thêm cargo mới
        /// </summary>
        /// <param name="cargoRequest">Thông tin cargo</param>
        /// <returns>Cargo đã tạo</returns>
        [HttpPost("createcargo")]
        public async Task<ActionResult<object>> CreateCargo([FromBody] CreateCargoRequest cargoRequest)
        {
            try
            {
                if (string.IsNullOrEmpty(cargoRequest.CustomerCompanyName))
                {
                    return BadRequest(new { success = false, message = "Tên công ty khách hàng không được để trống" });
                }

                if (string.IsNullOrEmpty(cargoRequest.EmployeeCreate))
                {
                    return BadRequest(new { success = false, message = "Tên nhân viên tạo không được để trống" });
                }

                if (string.IsNullOrEmpty(cargoRequest.CustomerPersonInCharge))
                {
                    return BadRequest(new { success = false, message = "Người phụ trách khách hàng không được để trống" });
                }

                var cargoDto = MapToCargoDto(cargoRequest);
                var cargo = await _cargoService.CreateCargoAsync(cargoDto);

                return CreatedAtAction(nameof(GetCargoById),
                    new { id = cargo.Id },
                    new { success = true, data = cargo, message = "Tạo cargo thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo cargo mới");
                return StatusCode(500, new { success = false, message = "Lỗi server khi tạo cargo" });
            }
        }

        /// <summary>
        /// Cập nhật cargo
        /// </summary>
        /// <param name="id">ID cargo</param>
        /// <param name="cargoRequest">Thông tin cargo cập nhật</param>
        /// <returns>Cargo đã cập nhật</returns>
        [HttpPut("updatecargo/{id}")]
        public async Task<ActionResult<object>> UpdateCargo(int id, [FromBody] UpdateCargoRequest cargoRequest)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { success = false, message = "ID cargo không hợp lệ" });
                }

                var cargoDto = MapToCargoDto(cargoRequest);
                var updatedCargo = await _cargoService.UpdateCargoAsync(id, cargoDto);
                if (updatedCargo == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy cargo" });
                }

                return Ok(new { success = true, data = updatedCargo, message = "Cập nhật cargo thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật cargo ID: {CargoId}", id);
                return StatusCode(500, new { success = false, message = "Lỗi server khi cập nhật cargo" });
            }
        }

        /// <summary>
        /// Xóa cargo
        /// </summary>
        /// <param name="id">ID cargo</param>
        /// <returns>Kết quả xóa</returns>
        [HttpDelete("deletecargo/{id}")]
        public async Task<ActionResult<object>> DeleteCargo(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { success = false, message = "ID cargo không hợp lệ" });
                }

                var result = await _cargoService.DeleteCargoAsync(id);
                if (!result)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy cargo" });
                }

                return Ok(new { success = true, message = $"Xóa cargo #{id} thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa cargo ID: {CargoId}", id);
                return StatusCode(500, new { success = false, message = "Lỗi server khi xóa cargo" });
            }
        }





        [HttpGet("exportcargodataexcel")]
        public async Task<ActionResult<object>> ExportCargoDataExcel([FromQuery] DateTime? datebegin, [FromQuery] DateTime? dateend)
        {
            try
            {
                var fileContent = await _cargoService.ExportCargoDataToExcelAsync(datebegin, dateend);
                
                string fileName;
                if (datebegin.HasValue && dateend.HasValue)
                {
                    fileName = $"BaoCaoLogistics_{datebegin:ddMMyyyy}_{dateend:ddMMyyyy}.xlsx";
                }
                else
                {
                    fileName = $"BaoCaoLogistics_All_{DateTime.Now:ddMMyyyy_HHmm}.xlsx";
                }
                
                return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xuất dữ liệu Excel: {DateBegin} - {DateEnd}", datebegin, dateend);
                return StatusCode(500, new { success = false, message = "Lỗi server khi xuất dữ liệu Excel" });
            }
        }

        /// <summary>
        /// Cập nhật file JSON cho cargo (background task)
        /// </summary>
        /// <param name="id">ID cargo</param>
        /// <returns>Kết quả</returns>
        [HttpPost("updatecargofile/{id}")]
        public async Task<ActionResult<object>> UpdateCargoFile(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { success = false, message = "ID cargo không hợp lệ" });
                }

                // Lấy cargo từ database
                var cargo = await _cargoService.GetCargoByIdAsync(id);
                if (cargo == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy cargo" });
                }

                // Gọi service để update file (background task)
                await _cargoService.UpdateCargoFileBackgroundAsync(id);

                return Ok(new { success = true, message = "Đang cập nhật file cargo trong background" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi update file cargo ID: {CargoId}", id);
                return StatusCode(500, new { success = false, message = "Lỗi server khi update file" });
            }
        }

        /// <summary>
        /// Tìm kiếm cargo theo mã cargo
        /// </summary>
        /// <param name="cargoCode">Mã cargo</param>
        /// <returns>Danh sách cargo</returns>
        [HttpGet("searchcargobynumandcus")]
        public async Task<ActionResult<object>> searchcargobynumandcus([FromQuery] string stringsearch)
        {
            try
            {
                if (string.IsNullOrEmpty(stringsearch))
                {
                    return BadRequest(new { success = false, message = "Từ khóa tìm kiếm không được để trống" });
                }

                var cargos = await _cargoService.SearchCargoByNumAsync(stringsearch);
                return Ok(new { success = true, data = cargos, message = $"Tìm thấy {cargos.Count()} cargo với mã '{stringsearch}'" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm cargo theo mã: {CargoCode}", stringsearch);
                return StatusCode(500, new { success = false, message = "Lỗi server khi tìm kiếm cargo" });
            }
        }

        /// <summary>
        /// Lấy danh sách cargo theo shipper ID
        /// </summary>
        /// <param name="shipperId">ID của shipper</param>
        /// <returns>Danh sách cargo của shipper</returns>
        [HttpGet("getcargobyshipperid/{shipperId}")]
        public async Task<ActionResult<object>> GetCargoByShipperId(int shipperId)
        {
            try
            {
                if (shipperId <= 0)
                {
                    return BadRequest(new { success = false, message = "Shipper ID không hợp lệ" });
                }

                var cargos = await _cargoService.GetCargoByShipperIdAsync(shipperId);
                return Ok(new { success = true, data = cargos, message = $"Lấy danh sách cargo của shipper #{shipperId} thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách cargo của shipper ID: {ShipperId}", shipperId);
                return StatusCode(500, new { success = false, message = "Lỗi server khi lấy danh sách cargo" });
            }
        }

        /// <summary>
        /// Chuyển đổi CreateCargoRequest thành CargoDto
        /// </summary>
        private CargoDto MapToCargoDto(CreateCargoRequest request)
        {
            return new CargoDto
            {
                CargoCode = request.CargoCode,
                CustomerCompanyName = request.CustomerCompanyName,
                EmployeeCreate = request.EmployeeCreate,
                CustomerPersonInCharge = request.CustomerPersonInCharge,
                CustomerAddress = request.CustomerAddress,
                ServiceType = request.ServiceType,
                ExchangeDate = request.ExchangeDate,
                EstimatedTotalAmount = request.EstimatedTotalAmount,
                AdvanceMoney = request.AdvanceMoney,
                ShippingFee = request.ShippingFee,
                IdShipper = request.IdShipper,
                StatusCargo = request.StatusCargo ?? 1
            };
        }

        /// <summary>
        /// Chuyển đổi UpdateCargoRequest thành CargoDto
        /// </summary>
        private CargoDto MapToCargoDto(UpdateCargoRequest request)
        {
            return new CargoDto
            {
                CargoCode = request.CargoCode,
                CustomerCompanyName = request.CustomerCompanyName,
                EmployeeCreate = request.EmployeeCreate,
                CustomerPersonInCharge = request.CustomerPersonInCharge,
                CustomerAddress = request.CustomerAddress,
                ServiceType = request.ServiceType,
                ExchangeDate = request.ExchangeDate,
                EstimatedTotalAmount = request.EstimatedTotalAmount,
                AdvanceMoney = request.AdvanceMoney,
                ShippingFee = request.ShippingFee,
                IdShipper = request.IdShipper,
                StatusCargo = request.StatusCargo ?? 1
            };
        }

        /// <summary>
        /// Chuyển đổi Cargolist thành CargoResponse
        /// </summary>
        private CargoResponse MapToCargoResponse(logistic_web.infrastructure.Models.Cargolist cargo)
        {
            return new CargoResponse
            {
                Id = cargo.Id,
                CargoCode = cargo.CargoCode,
                CustomerCompanyName = cargo.CustomerCompanyName,
                EmployeeCreate = cargo.EmployeeCreate,
                CustomerPersonInCharge = cargo.CustomerPersonInCharge,
                CustomerAddress = cargo.CustomerAddress,
                ServiceType = cargo.ServiceType,
                LicenseDate = cargo.LicenseDate,
                ExchangeDate = cargo.ExchangeDate,
                EstimatedTotalAmount = cargo.EstimatedTotalAmount,
                AdvanceMoney = cargo.AdvanceMoney,
                ShippingFee = cargo.ShippingFee,
                IdShipper = cargo.IdShipper,
                StatusCargo = cargo.StatusCargo,
                CreatedAt = cargo.CreatedAt,
                FilePathJson = cargo.FilePathJson
            };
        }


        /// <summary>
        /// Lấy thống kê tổng hợp trong tháng hiện tại (Số lượng đơn hàng + Tổng doanh thu)
        /// GET: api/cargo/monthlystatistics
        /// </summary>
        /// <returns>Thống kê tổng hợp: số lượng đơn hàng và tổng doanh thu</returns>
        [HttpGet("monthlystatistics")]
        public async Task<ActionResult<DashboardVM>> GetMonthlyStatistics()
        {
             try
    {
        var now = DateTime.Now;
        var statistics = await _cargoService.GetMonthlyStatisticsAsync();

        var cargoCount = statistics.count;
        var totalRevenue = statistics.totalRevenue;
        var averageRevenuePerCargo = cargoCount > 0 ? totalRevenue / cargoCount : 0;

        var result = new DashboardVM
        {
            CargoCount = cargoCount,
            TotalRevenue = totalRevenue,
            TotalRevenueFormatted = $"{totalRevenue:N0} VND",
            AverageRevenuePerCargo = averageRevenuePerCargo,
            AverageRevenueFormatted = $"{averageRevenuePerCargo:N0} VND",
            Month = now.Month,
            Year = now.Year,
            MonthName = $"Tháng {now.Month}/{now.Year}",
            StartDate = new DateTime(now.Year, now.Month, 1).ToString("dd/MM/yyyy"),
            EndDate = new DateTime(now.Year, now.Month, 1)
                        .AddMonths(1)
                        .AddDays(-1)
                        .ToString("dd/MM/yyyy")
        };

        return Ok(result);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Lỗi khi lấy thống kê tháng hiện tại");
        return StatusCode(500, new { success = false, message = "Lỗi server khi lấy thống kê" });
    }
        }
    }
}

