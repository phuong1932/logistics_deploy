using Microsoft.AspNetCore.Mvc;
using logistic_web.application.Services;
using logistic_web.application.DTO;
using Microsoft.AspNetCore.Authorization;

namespace logistic_web.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShipperController : ControllerBase
    {
        private readonly IShipperService _shipperService;
        private readonly ILogger<ShipperController> _logger;

        public ShipperController(
            IShipperService shipperService,
            ILogger<ShipperController> logger)
        {
            _shipperService = shipperService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách tất cả shippers
        /// </summary>
        [HttpGet("getallshippers")]
        public async Task<ActionResult<object>> GetAllShippers()
        {
            try
            {
                var shippers = await _shipperService.GetAllShippersAsync();
                return Ok(new { success = true, data = shippers, message = "Lấy danh sách shipper thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách shipper");
                return StatusCode(500, new { success = false, message = "Lỗi server khi lấy danh sách shipper" });
            }
        }

        /// <summary>
        /// Lấy thông tin shipper theo ID
        /// </summary>
        [HttpGet("getshipperbyid/{id}")]
        public async Task<ActionResult<object>> GetShipperById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { success = false, message = "ID shipper không hợp lệ" });
                }

                var shipper = await _shipperService.GetShipperByIdAsync(id);
                if (shipper == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy shipper" });
                }

                return Ok(new { success = true, data = shipper, message = "Lấy thông tin shipper thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy shipper ID: {ShipperId}", id);
                return StatusCode(500, new { success = false, message = "Lỗi server khi lấy thông tin shipper" });
            }
        }

        /// <summary>
        /// Tạo shipper mới
        /// </summary>
        [HttpPost("createshipper")]
        public async Task<ActionResult<object>> CreateShipper([FromBody] CreateShipperRequest shipperRequest)
        {
            try
            {
                if (!shipperRequest.LoaiXe.HasValue || shipperRequest.LoaiXe < 0 || shipperRequest.LoaiXe > 2)
                {
                    return BadRequest(new { success = false, message = "Vui lòng chọn loại xe hợp lệ" });
                }

                var shipperDto = new ShipperDto
                {
                    TenTaiXe = shipperRequest.TenTaiXe,
                    LoaiXe = shipperRequest.LoaiXe,
                    SoDienThoai = shipperRequest.SoDienThoai,
                    DiaChi = shipperRequest.DiaChi
                };

                var shipper = await _shipperService.CreateShipperAsync(shipperDto);

                return CreatedAtAction(nameof(GetShipperById),
                    new { id = shipper.Id },
                    new { success = true, data = shipper, message = "Tạo shipper thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo shipper mới");
                return StatusCode(500, new { success = false, message = "Lỗi server khi tạo shipper" });
            }
        }

        /// <summary>
        /// Cập nhật shipper
        /// </summary>
        [HttpPut("updateshipper/{id}")]
        public async Task<ActionResult<object>> UpdateShipper(int id, [FromBody] UpdateShipperRequest shipperRequest)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { success = false, message = "ID shipper không hợp lệ" });
                }
                  if (!shipperRequest.LoaiXe.HasValue || shipperRequest.LoaiXe < 0 || shipperRequest.LoaiXe > 2)
                {
                    return BadRequest(new { success = false, message = "Vui lòng chọn loại xe hợp lệ" });
                }

                var shipperDto = new ShipperDto
                {
                    TenTaiXe = shipperRequest.TenTaiXe,
                    LoaiXe = shipperRequest.LoaiXe,
                    SoDienThoai = shipperRequest.SoDienThoai,
                    DiaChi = shipperRequest.DiaChi
                };

                var updatedShipper = await _shipperService.UpdateShipperAsync(id, shipperDto);
                if (updatedShipper == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy shipper" });
                }

                return Ok(new { success = true, data = updatedShipper, message = "Cập nhật shipper thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật shipper ID: {ShipperId}", id);
                return StatusCode(500, new { success = false, message = "Lỗi server khi cập nhật shipper" });
            }
        }

        /// <summary>
        /// Xóa shipper
        /// </summary>
        [HttpDelete("deleteshipper/{id}")]
        public async Task<ActionResult<object>> DeleteShipper(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { success = false, message = "ID shipper không hợp lệ" });
                }

                var result = await _shipperService.DeleteShipperAsync(id);
                if (!result)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy shipper" });
                }

                return Ok(new { success = true, message = $"Xóa shipper #{id} thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa shipper ID: {ShipperId}", id);
                return StatusCode(500, new { success = false, message = "Lỗi server khi xóa shipper" });
            }
        }

        /// <summary>
        /// Tìm kiếm shipper theo tên
        /// </summary>
        [HttpGet("searchshipper")]
        public async Task<ActionResult<object>> SearchShipperByName([FromQuery] string name)
        {
            try
            {
                if (string.IsNullOrEmpty(name))
                {
                    return BadRequest(new { success = false, message = "Tên tài xế không được để trống" });
                }

                var shippers = await _shipperService.SearchShipperByNameAsync(name);
                return Ok(new { success = true, data = shippers, message = $"Tìm thấy {shippers.Count()} shipper" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm shipper theo tên: {Name}", name);
                return StatusCode(500, new { success = false, message = "Lỗi server khi tìm kiếm shipper" });
            }
        }
    
        /// <summary>
        /// Lấy tên shipper theo ID
        /// </summary>
        [HttpGet("getshippernamebyid/{id}")]
        public async Task<ActionResult<object>> GetShipperNameById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { success = false, message = "ID shipper không hợp lệ" });
                }

                var name = await _shipperService.GetShipperNameByIdAsync(id);
                if (string.IsNullOrEmpty(name))
                {
                    return NotFound(new { success = false, message = "Không tìm thấy shipper" });
                }

                return Ok(name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy tên shipper theo ID: {ShipperId}", id);
                return StatusCode(500, new { success = false, message = "Lỗi server khi lấy tên shipper" });
            }
        }
    }
}

