using logistic_web.infrastructure.Models;
using logistic_web.infrastructure.Repositories;
using logistic_web.infrastructure.Unitofwork;
using logistic_web.application.DTO;

namespace logistic_web.application.Services
{
    public interface IRoleService
    {
        Task<RoleResponse> CreateRoleAsync(CreateRoleRequest model);
        Task<bool> UpdateRoleAsync(int roleId, UpdateRoleRequest model);
        Task<bool> DeleteRoleAsync(int roleId);
        Task<RoleResponse> GetRoleByIdAsync(int roleId);
        Task<RoleResponse> GetRoleByNameAsync(string roleName);
        Task<IEnumerable<RoleResponse>> GetAllRolesAsync();
        Task<bool> RoleExistsAsync(string roleName);
    }

    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RoleService> _logger;

        public RoleService(
            IRoleRepository roleRepository,
            IUnitOfWork unitOfWork,
            ILogger<RoleService> logger)
        {
            _roleRepository = roleRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<RoleResponse> CreateRoleAsync(CreateRoleRequest model)
        {
            try
            {
                // Kiểm tra role đã tồn tại chưa
                if (await _roleRepository.ExistsByNameAsync(model.RoleName))
                {
                    _logger.LogWarning("Role already exists: {RoleName}", model.RoleName);
                    return null;
                }

                var role = new Role
                {
                    RoleName = model.RoleName,
                    Description = model.Description,
                    CreatedAt = DateTime.Now
                };

                await _roleRepository.AddAsync(role);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Role created successfully: {RoleName}", role.RoleName);
                return new RoleResponse
                {
                    Id = role.Id,
                    RoleName = role.RoleName,
                    Description = role.Description,
                    CreatedAt = role.CreatedAt ?? DateTime.Now
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role: {RoleName}", model.RoleName);
                return null;
            }
        }

        public async Task<bool> UpdateRoleAsync(int roleId, UpdateRoleRequest model)
        {
            try
            {
                var role = await _roleRepository.GetByIdAsync(roleId);
                if (role == null)
                {
                    _logger.LogWarning("Role not found for update: {RoleId}", roleId);
                    return false;
                }

                // Kiểm tra tên role mới có trùng với role khác không
                if (model.RoleName != role.RoleName && await _roleRepository.ExistsByNameAsync(model.RoleName))
                {
                    _logger.LogWarning("Role name already exists: {RoleName}", model.RoleName);
                    return false;
                }

                role.RoleName = model.RoleName;
                role.Description = model.Description;

                _roleRepository.Update(role);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Role updated successfully: {RoleId}", roleId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role: {RoleId}", roleId);
                return false;
            }
        }

        public async Task<bool> DeleteRoleAsync(int roleId)
        {
            try
            {
                var role = await _roleRepository.GetByIdAsync(roleId);
                if (role == null)
                {
                    _logger.LogWarning("Role not found for deletion: {RoleId}", roleId);
                    return false;
                }

                // Kiểm tra xem role có đang được sử dụng không
                var userRoles = await _roleRepository.GetRolesByUserIdAsync(roleId);
                if (userRoles.Any())
                {
                    _logger.LogWarning("Cannot delete role that is in use: {RoleId}", roleId);
                    return false;
                }

                _roleRepository.Remove(role);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Role deleted successfully: {RoleId}", roleId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting role: {RoleId}", roleId);
                return false;
            }
        }

        public async Task<RoleResponse> GetRoleByIdAsync(int roleId)
        {
            try
            {
                var role = await _roleRepository.GetByIdAsync(roleId);
                if (role == null)
                {
                    return null;
                }

                return new RoleResponse
                {
                    Id = role.Id,
                    RoleName = role.RoleName,
                    Description = role.Description,
                    CreatedAt = role.CreatedAt ?? DateTime.Now
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting role: {RoleId}", roleId);
                return null;
            }
        }

        public async Task<RoleResponse> GetRoleByNameAsync(string roleName)
        {
            try
            {
                var role = await _roleRepository.GetByNameAsync(roleName);
                if (role == null)
                {
                    return null;
                }

                return new RoleResponse
                {
                    Id = role.Id,
                    RoleName = role.RoleName,
                    Description = role.Description,
                    CreatedAt = role.CreatedAt ?? DateTime.Now
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting role by name: {RoleName}", roleName);
                return null;
            }
        }

        public async Task<IEnumerable<RoleResponse>> GetAllRolesAsync()
        {
            try
            {
                var roles = await _roleRepository.GetAllAsync();
                return roles.Select(role => new RoleResponse
                {
                    Id = role.Id,
                    RoleName = role.RoleName,
                    Description = role.Description,
                    CreatedAt = role.CreatedAt ?? DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all roles");
                return new List<RoleResponse>();
            }
        }

        public async Task<bool> RoleExistsAsync(string roleName)
        {
            try
            {
                return await _roleRepository.ExistsByNameAsync(roleName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if role exists: {RoleName}", roleName);
                return false;
            }
        }
    }
}
