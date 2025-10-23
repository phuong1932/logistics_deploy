using Microsoft.AspNetCore.Mvc;
using logistic_web.application.Services;
using logistic_web.application.DTO;
using Microsoft.AspNetCore.Authorization;

namespace logistic_web.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(
            ICustomerService customerService,
            ILogger<CustomerController> logger)
        {
            _customerService = customerService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách tất cả customers
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllCustomers()
        {
            try
            {
                var customers = await _customerService.GetAllCustomersAsync();
                return Ok(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in get all customers endpoint");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }

        /// <summary>
        /// Lấy thông tin customer theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomerById(int id)
        {
            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(id);
                if (customer == null)
                {
                    return NotFound(new { message = "Không tìm thấy customer" });
                }

                return Ok(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in get customer by id endpoint");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }

        /// <summary>
        /// Tạo customer mới
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _customerService.CreateCustomerAsync(model);
                if (!result)
                {
                    return BadRequest(new { message = "Tạo customer thất bại" });
                }

                return Ok(new { message = "Tạo customer thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in create customer endpoint");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }

        /// <summary>
        /// Cập nhật thông tin customer
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] UpdateCustomerRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _customerService.UpdateCustomerAsync(id, model);
                if (!result)
                {
                    return BadRequest(new { message = "Cập nhật customer thất bại" });
                }

                return Ok(new { message = "Cập nhật customer thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in update customer endpoint");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }

        /// <summary>
        /// Xóa customer
        /// </summary>
        [HttpDelete("{id}")]

        public async Task<IActionResult> DeleteCustomer(int id)
        {
            try
            {
                var result = await _customerService.DeleteCustomerAsync(id);
                if (!result)
                {
                    return BadRequest(new { message = "Xóa customer thất bại" });
                }

                return Ok(new { message = "Xóa customer thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in delete customer endpoint");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }

        /// <summary>
        /// Tìm kiếm customer theo tên
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchCustomerByName([FromQuery] string name)
        {
            try
            {
                if (string.IsNullOrEmpty(name))
                {
                    return BadRequest(new { message = "Tên khách hàng không được để trống" });
                }

                var customers = await _customerService.SearchCustomerByNameAsync(name);
                return Ok(new { success = true, data = customers, message = $"Tìm thấy {customers.Count()} khách hàng" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in search customer endpoint");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }
    }
}
