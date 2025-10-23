using Microsoft.AspNetCore.Mvc;
using logistic_web.application.Services;
using logistic_web.application.DTO;
using logistic_web.application.Helpers;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace logistic_web.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly ILogger<UserController> _logger;

        public UserController(
            IUserService userService,
            IRoleService roleService,
            ILogger<UserController> logger)
        {
            _userService = userService;
            _roleService = roleService;
            _logger = logger;
        }

        /// <summary>
        /// Đăng nhập user
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _userService.LoginAsync(model);
                
                if (result == MessageLogin.UserNotFound)
                {
                    return NotFound(new { message = MessageLogin.UserNotFound });
                }
                
                if (result == MessageLogin.PasswordIncorrect)
                {
                    return Unauthorized(new { message = MessageLogin.PasswordIncorrect });
                }
                
                if (result == MessageLogin.ErrorInServer)
                {
                    return StatusCode(500, new { message = MessageLogin.ErrorInServer });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in login endpoint");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }

        /// <summary>
        /// Đăng ký user mới
        /// </summary>
        [Authorize(Roles = "admin")]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterRequest model) 
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _userService.RegisterAsync(model);
                
                if (!result)
                {
                    return BadRequest(new { message = MessageLogin.UserAlreadyExists });
                }

                return Ok(new { message = MessageLogin.RegisterSuccess });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in register endpoint");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }

        /// <summary>
        /// Lấy thông tin user hiện tại
        /// </summary>
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Token không hợp lệ" });
                }

                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "Không tìm thấy user" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in get profile endpoint");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }

        /// <summary>
        /// Cập nhật thông tin user hiện tại
        /// </summary>
    
        /// <summary>
        /// Lấy danh sách tất cả users (chỉ admin)
        /// </summary>
        [Authorize(Roles = "admin")]
        [HttpGet("getallusers")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in get all users endpoint");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }

        /// <summary>
        /// Lấy thông tin user theo ID (chỉ admin)
        /// </summary>
        [Authorize(Roles = "admin")]
        [HttpGet("getuserbyid/{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "Không tìm thấy user" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in get user by id endpoint");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }

        /// <summary>
        /// Cập nhật user (chỉ admin)
        /// </summary>
        [Authorize(Roles = "admin")]
        [HttpPut("updateuser/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _userService.UpdateUserAsync(id, model);
                if (!result)
                {
                    return BadRequest(new { message = "Cập nhật thất bại" });
                }

                return Ok(new { message = "Cập nhật thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in update user endpoint");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }

        /// <summary>
        /// Xóa user (chỉ admin)
        /// </summary>
        [Authorize(Roles = "admin")]
        [HttpDelete("deleteuser/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var result = await _userService.DeleteUserAsync(id);
                if (!result)
                {
                    return BadRequest(new { message = "Xóa thất bại" });
                }

                return Ok(new { message = "Xóa thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in delete user endpoint");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }

        /// <summary>
        /// Gán role cho user (chỉ admin)
        /// </summary>
        [Authorize(Roles = "admin")]
        [HttpPost("assignrole")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _userService.AssignRoleToUserAsync(model.UserId, model.RoleId);
                if (!result)
                {
                    return BadRequest(new { message = "Gán role thất bại" });
                }

                return Ok(new { message = "Gán role thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in assign role endpoint");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }

        /// <summary>
        /// Xóa role khỏi user (chỉ admin)
        /// </summary>
        [Authorize(Roles = "admin")]
        [HttpDelete("removerole")]
        public async Task<IActionResult> RemoveRole([FromBody] AssignRoleRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _userService.RemoveRoleFromUserAsync(model.UserId, model.RoleId);
                if (!result)
                {
                    return BadRequest(new { message = "Xóa role thất bại" });
                }

                return Ok(new { message = "Xóa role thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in remove role endpoint");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }
    }
}
