using Microsoft.AspNetCore.Mvc;
using logistic_web.application.Services;
using Microsoft.AspNetCore.Authorization;

namespace logistic_web.api.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserRoleController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly ILogger<UserRoleController> _logger;

        public UserRoleController(
            IUserService userService,
            IRoleService roleService,
            ILogger<UserRoleController> logger)
        {
            _userService = userService;
            _roleService = roleService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách roles của user theo userId
        /// </summary>
        [HttpGet("{userId}/roles")]
        public async Task<IActionResult> GetUserRoles(int userId)
        {
            try
            {
                // Lấy thông tin user
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "User không tồn tại" });
                }

                // Lấy roles của user (cần implement method này trong UserService)
                var roles = await GetUserRoleNamesAsync(userId);
                
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roles for user {UserId}", userId);
                return StatusCode(500, new { message = "Lỗi server khi lấy roles" });
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
