using Microsoft.AspNetCore.Mvc;
using logistic_web.application.Services;
using logistic_web.application.DTO;
using Microsoft.AspNetCore.Authorization;

namespace logistic_web.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "admin")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly ILogger<RoleController> _logger;

        public RoleController(IRoleService roleService, ILogger<RoleController> logger)
        {
            _roleService = roleService;
            _logger = logger;
        }

        /// <summary>
        /// Tạo role mới
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _roleService.CreateRoleAsync(model);
                if (result == null)
                {
                    return BadRequest(new { message = "Tạo role thất bại. Có thể tên role đã tồn tại." });
                }

                return CreatedAtAction(nameof(GetRoleById), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in create role endpoint");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }

        /// <summary>
        /// Lấy tất cả roles
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            try
            {
                var roles = await _roleService.GetAllRolesAsync();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in get all roles endpoint");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }

        /// <summary>
        /// Lấy role theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoleById(int id)
        {
            try
            {
                var role = await _roleService.GetRoleByIdAsync(id);
                if (role == null)
                {
                    return NotFound(new { message = "Không tìm thấy role" });
                }

                return Ok(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in get role by id endpoint");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }

        /// <summary>
        /// Lấy role theo tên
        /// </summary>
        [HttpGet("by-name/{roleName}")]
        public async Task<IActionResult> GetRoleByName(string roleName)
        {
            try
            {
                var role = await _roleService.GetRoleByNameAsync(roleName);
                if (role == null)
                {
                    return NotFound(new { message = "Không tìm thấy role" });
                }

                return Ok(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in get role by name endpoint");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }

        /// <summary>
        /// Cập nhật role
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _roleService.UpdateRoleAsync(id, model);
                if (!result)
                {
                    return BadRequest(new { message = "Cập nhật role thất bại" });
                }

                return Ok(new { message = "Cập nhật role thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in update role endpoint");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }

        /// <summary>
        /// Xóa role
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            try
            {
                var result = await _roleService.DeleteRoleAsync(id);
                if (!result)
                {
                    return BadRequest(new { message = "Xóa role thất bại. Có thể role đang được sử dụng." });
                }

                return Ok(new { message = "Xóa role thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in delete role endpoint");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }

        /// <summary>
        /// Kiểm tra role có tồn tại không
        /// </summary>
        [HttpGet("exists/{roleName}")]
        public async Task<IActionResult> RoleExists(string roleName)
        {
            try
            {
                var exists = await _roleService.RoleExistsAsync(roleName);
                return Ok(new { exists = exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in role exists endpoint");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }
    }
}
