using Microsoft.AspNetCore.Mvc;
using logistic_web.application.Services;
using Microsoft.AspNetCore.Authorization;
using logistic_web.api.DTO;

namespace logistic_web.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserRoleController : ControllerBase
    {
        private readonly IUserRoleService _userRoleService;
        private readonly IRoleService _roleService;
        private readonly ILogger<UserRoleController> _logger;

        public UserRoleController(
            IUserRoleService userRoleService,
            IRoleService roleService,
            ILogger<UserRoleController> logger)
        {
            _userRoleService = userRoleService;
                _roleService = roleService;
            _logger = logger;
        }


        [HttpGet("GetUserRolesbyuserid/{id}")]
        public async Task<IActionResult> GetUserRolesByUserId(int id)
        {
            try
            {
                var userroles = await _userRoleService.GetUserRolesByUserIdAsync(id);
                if (userroles == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy user roles" });
                }

                return Ok(new 
                { 
                    success = true, 
                    data = new List<object> { userroles }, 
                    message = "Lấy user roles thành công" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in get user roles by id endpoint");
                return StatusCode(500, new { success = false, message = "Lỗi server" });
            }
        }

        /// <summary>
        /// Lấy danh sách roles của user theo userId
        /// </summary>
        [HttpGet("getuserroles/{id}")]
        public async Task<IActionResult> GetUserRoles(int userId)
        {
            try
            {
            
                // Lấy roles của user (cần implement method này trong UserService)
                var roles = await GetUserRoleNamesAsync(userId);
                return Ok(new { success = true, data = roles, message = "Lấy danh sách roles thành công" });
    
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roles for user {UserId}", userId);
                return StatusCode(500, new { message = "Lỗi server khi lấy roles" });
            }
        }

        /// <summary>
        /// Cập nhật role và shipper của user
        /// </summary>
        [HttpPut("update/{userId}")]
        public async Task<IActionResult> UpdateUserRole(int userId, [FromBody] UpdateUserRoleRequest request)
        {
            try
            {
                if (request.RoleId <= 0)
                {
                    return BadRequest(new { success = false, message = "RoleId không hợp lệ" });
                }

                var result = await _userRoleService.UpdateUserRoleAsync(userId, request.RoleId, request.ShipperId);
                
                if (!result)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy user role để cập nhật" });
                }

                return Ok(new 
                { 
                    success = true, 
                    message = "Cập nhật role thành công",
                    data = new 
                    {
                        userId = userId,
                        roleId = request.RoleId,
                        shipperId = request.ShipperId
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user role for user {UserId}", userId);
                return StatusCode(500, new { success = false, message = "Lỗi server" });
            }
        }

        private async Task<List<string>> GetUserRoleNamesAsync(int userId)
        {
            try
            {
                // Gọi RoleService để lấy roles của user
                // Bạn cần implement method này trong RoleService
                var userRoles = await _roleService.GetRoleByIdAsync(userId);
                return new List<string> { userRoles?.RoleName ?? "user" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting role names for user {UserId}", userId);
                return new List<string> { "user" }; // Default role
            }
        }
    }

 
}
