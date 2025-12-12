using logistic_web.infrastructure.Models;
using logistic_web.infrastructure.Unitofwork;
using logistic_web.application.DTO;

namespace logistic_web.application.Services
{
    public interface IUserRoleService
    {
        Task<UserRole> GetUserRolesByUserIdAsync(int userId);
        Task<bool> UpdateUserRoleAsync(int userId, int roleId, int? shipperId);
    }

    public class UserRoleService : IUserRoleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserRoleService> _logger;

        public UserRoleService(
            IUnitOfWork unitOfWork,
            ILogger<UserRoleService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

    public async Task<UserRole?> GetUserRolesByUserIdAsync(int userId)
    {
        try
        {
            var userRoles = await _unitOfWork.UserRoleRepository.SingleOrDefaultAsync(ur => ur.UserId == userId);
            return userRoles;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user roles for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> UpdateUserRoleAsync(int userId, int roleId, int? shipperId)
    {
        try
        {
            var userRole = await _unitOfWork.UserRoleRepository.SingleOrDefaultAsync(ur => ur.UserId == userId);
            
            if (userRole == null)
            {
                _logger.LogWarning("UserRole not found for userId: {UserId}", userId);
                return false;
            }

            // Update role và shipper
            userRole.RoleId = roleId;
            userRole.ShipperId = shipperId;

            _unitOfWork.UserRoleRepository.Update(userRole);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Updated user role: UserId={UserId}, RoleId={RoleId}, ShipperId={ShipperId}", 
                userId, roleId, shipperId);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user role for user: {UserId}", userId);
            throw;
        }
    }
    }
}
