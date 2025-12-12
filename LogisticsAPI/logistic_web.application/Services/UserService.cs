using logistic_web.infrastructure.Models;
using logistic_web.infrastructure.Repositories;
using logistic_web.infrastructure.Unitofwork;
using logistic_web.application.Helpers;
using logistic_web.application.DTO;

namespace logistic_web.application.Services
{
    public interface IUserService
    {
        Task<string> LoginAsync(UserLoginRequest model);
        Task<bool> RegisterAsync(UserRegisterRequest model);
        Task<bool> UpdateUserAsync(int userId, UserUpdateRequest model);
        Task<bool> DeleteUserAsync(int userId);
        Task<UserResponse> GetUserByIdAsync(int userId);
        Task<IEnumerable<UserResponse>> GetAllUsersAsync();
        Task<IEnumerable<UserResponse>> GetUsersByCurrentUserRoleAsync(string role);
        Task<bool> AssignRoleToUserAsync(int userId, int roleId);
        Task<bool> RemoveRoleFromUserAsync(int userId, int roleId);
    }

    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly JwtAuthService _jwtAuthService;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUnitOfWork unitOfWork,
            JwtAuthService jwtAuthService,
            ILogger<UserService> logger)
        {
            _unitOfWork = unitOfWork;
            _jwtAuthService = jwtAuthService;
            _logger = logger;
        }

        public async Task<string> LoginAsync(UserLoginRequest model)
        {
            try
            {
                var user = await _unitOfWork.UserRepository.GetByUsernameAsync(model.UsernameOrEmail) 
                          ?? await _unitOfWork.UserRepository.GetByEmailAsync(model.UsernameOrEmail);
                
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UsernameOrEmail}", model.UsernameOrEmail);
                    return MessageLogin.UserNotFound;
                }

                if (!PasswordHelper.VerifyPassword(model.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Invalid password for user: {Username}", user.Username);
                    return MessageLogin.PasswordIncorrect;
                }

                _logger.LogInformation("User logged in successfully: {Username}", user.Username);
                return _jwtAuthService.GenerateToken(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user: {UsernameOrEmail}", model.UsernameOrEmail);
                return MessageLogin.ErrorInServer;
            }
        }

        public async Task<bool> RegisterAsync(UserRegisterRequest model)
        {
            try
            {
                // Kiểm tra user đã tồn tại chưa
                var existingUser = await _unitOfWork.UserRepository.GetByUsernameAsync(model.Username) 
                                 ?? await _unitOfWork.UserRepository.GetByEmailAsync(model.Email);
                
                if (existingUser != null)
                {
                    _logger.LogWarning("User already exists: {Username} or {Email}", model.Username, model.Email);
                    return false;
                }

                // Tạo user mới
                var user = new User
                {
                    Username = model.Username,
                    Email = model.Email,
                    PasswordHash = PasswordHelper.HashPassword(model.Password),
                    FullName = model.FullName,
                    CreatedAt = DateTime.Now,
                    Deleted = false
                };

                await _unitOfWork.UserRepository.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();

                // Gán role mặc định (user) nếu không chỉ định role
                if (model.RoleId == 0)
                {
                    var userRole = await _unitOfWork.RoleRepository.GetByNameAsync("user");
                    if (userRole != null)
                    {
                        var userRoleEntity = new UserRole
                        {
                            UserId = user.Id,
                            RoleId = userRole.Id,
                            ShipperId = model.ShipperId,
                            Description = user.Id.ToString()
                        };
                        await _unitOfWork.UserRoleRepository.AddAsync(userRoleEntity);
                    }
                }
                else
                {
                    var userRoleEntity = new UserRole
                    {
                        UserId = user.Id,
                        RoleId = model.RoleId,
                        ShipperId = model.ShipperId,
                        Description = user.Id.ToString()
                    };
                    await _unitOfWork.UserRoleRepository.AddAsync(userRoleEntity);
                }

                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("User registered successfully: {Username}", user.Username);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for user: {Username}", model.Username);
                return false;
            }
        }

        public async Task<bool> UpdateUserAsync(int userId, UserUpdateRequest model)
        {
            try
            {
                var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for update: {UserId}", userId);
                    return false;
                }

                user.FullName = model.FullName;
                user.Email = model.Email;
                if (!string.IsNullOrEmpty(model.Password))
                {
                    user.PasswordHash = PasswordHelper.HashPassword(model.Password);
                }

                _unitOfWork.UserRepository.Update(user);
                await _unitOfWork.SaveChangesAsync();
                
                _logger.LogInformation("User updated successfully: {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            try
            {
                var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for deletion: {UserId}", userId);
                    return false;
                }

                // Xóa tất cả các bản ghi UserRole liên quan đến user này trước
                var userRoles = await _unitOfWork.UserRoleRepository.GetByUserId(userId);
                
                if (userRoles != null)
                {
                    _unitOfWork.UserRoleRepository.Remove(userRoles);
                }
                
                // Sau đó xóa user
                _unitOfWork.UserRepository.Remove(user);
                await _unitOfWork.SaveChangesAsync();
                
                _logger.LogInformation("User deleted successfully: {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {UserId}", userId);
                return false;
            }
        }

        public async Task<UserResponse> GetUserByIdAsync(int userId)
        {
            try
            {
                var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return null;
                }

                var userRoles = await _unitOfWork.UserRoleRepository.GetByUserIdAsync(userId);

                return new UserResponse
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.FullName,
                    CreatedAt = user.CreatedAt ?? DateTime.Now,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user: {UserId}", userId);
                return null;
            }
        }

        public async Task<IEnumerable<UserResponse>> GetAllUsersAsync()
        {
            try
            {
                var users = await _unitOfWork.UserRepository.GetAllAsync();
                var userResponses = new List<UserResponse>();

                foreach (var user in users.Where(u => u.Deleted != true))
                {
                    var userRoles = await _unitOfWork.UserRoleRepository.GetByUserIdAsync(user.Id);
                    var roleNames =  await _unitOfWork.RoleRepository.GetRoleNameByIdAsync(userRoles.Value);

                    userResponses.Add(new UserResponse
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Email = user.Email,
                        FullName = user.FullName,
                        CreatedAt = user.CreatedAt ?? DateTime.Now,
                        Roles = roleNames
                    });
                }

                return userResponses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                return new List<UserResponse>();
            }
        }

        public async Task<IEnumerable<UserResponse>> GetUsersByCurrentUserRoleAsync(string role)
        {
            try
            {
                List<User> filteredUsers = new List<User>();
                var roleLower = role?.ToLower() ?? "";

                // Nếu user hiện tại là admin: lấy tất cả users
                if (roleLower == "admin")
                {
                    var allUsers = await _unitOfWork.UserRepository.GetAllAsync();
                    filteredUsers = allUsers.Where(u => u.Deleted != true).ToList();
                }
                // Nếu user hiện tại là staff: chỉ lấy users có role shipper
                else if (roleLower == "staff")
                {
                    // Tìm role "shipper" trong database
                    var shipperRole = await _unitOfWork.RoleRepository.GetByNameAsync("shipper");
                    if (shipperRole != null)
                    {
                        // Lấy tất cả users có role shipper từ bảng UserRole
                        var shipperUsers = await _unitOfWork.UserRoleRepository.GetUsersByRoleAsync(shipperRole.Id);
                        filteredUsers = shipperUsers.Where(u => u.Deleted != true).ToList();
                    }
                }
                else
                {
                    // Mặc định: trả về danh sách rỗng
                    return new List<UserResponse>();
                }

                // Chuyển đổi sang UserResponse
                var userResponses = new List<UserResponse>();
                foreach (var user in filteredUsers)
                {
                    // Lấy tất cả roles của user
                    var userRolesList = await _unitOfWork.RoleRepository.GetRolesByUserIdAsync(user.Id);
                    var roleNames = string.Join(", ", userRolesList.Select(r => r.RoleName));

                    userResponses.Add(new UserResponse
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Email = user.Email,
                        FullName = user.FullName,
                        CreatedAt = user.CreatedAt ?? DateTime.Now,
                        Roles = roleNames
                    });
                }

                return userResponses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users by current user role");
                return new List<UserResponse>();
            }
        }

        public async Task<bool> AssignRoleToUserAsync(int userId, int roleId)
        {
            try
            {
                var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
                var role = await _unitOfWork.RoleRepository.GetByIdAsync(roleId);

                if (user == null || role == null)
                {
                    _logger.LogWarning("User or role not found: UserId={UserId}, RoleId={RoleId}", userId, roleId);
                    return false;
                }

                // var existingUserRole = await _unitOfWork.UserRoleRepository.GetByUserIdAsync(userId);
                // if (existingUserRole.Any(ur => ur.RoleId == roleId))
                // {
                //     _logger.LogWarning("User already has this role: UserId={UserId}, RoleId={RoleId}", userId, roleId);
                //     return false;
                // }

                var userRole = new UserRole
                {
                    UserId = userId,
                    RoleId = roleId,
                    Description = userId.ToString() // Có thể thay đổi thành ID của admin thực hiện
                };

                await _unitOfWork.UserRoleRepository.AddAsync(userRole);
                await _unitOfWork.SaveChangesAsync();
                
                _logger.LogInformation("Role assigned to user: UserId={UserId}, RoleId={RoleId}", userId, roleId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning role to user: UserId={UserId}, RoleId={RoleId}", userId, roleId);
                return false;
            }
        }

        public async Task<bool> RemoveRoleFromUserAsync(int userId, int roleId)
        {
            try
            {
                var userRoles = await _unitOfWork.UserRoleRepository.GetByUserIdAsync(userId);

                if (userRoles == null)
                {
                    _logger.LogWarning("User role not found: UserId={UserId}, RoleId={RoleId}", userId, roleId);
                    return false;
                }

                await _unitOfWork.UserRoleRepository.RemoveRoleFromUserAsync(userId, roleId);
                await _unitOfWork.SaveChangesAsync();
                
                _logger.LogInformation("Role removed from user: UserId={UserId}, RoleId={RoleId}", userId, roleId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing role from user: UserId={UserId}, RoleId={RoleId}", userId, roleId);
                return false;
            }
        }

    }
}
