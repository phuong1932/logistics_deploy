using System.ComponentModel.DataAnnotations;

namespace logistic_web.application.DTO
{
    public class AssignRoleRequest
    {
        [Required(ErrorMessage = "User ID là bắt buộc")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Role ID là bắt buộc")]
        public int RoleId { get; set; }
    }
}
