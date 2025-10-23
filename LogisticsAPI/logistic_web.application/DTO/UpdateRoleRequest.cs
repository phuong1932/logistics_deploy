using System.ComponentModel.DataAnnotations;

namespace logistic_web.application.DTO
{
    public class UpdateRoleRequest
    {
        [Required(ErrorMessage = "Tên role là bắt buộc")]
        [MaxLength(50, ErrorMessage = "Tên role không được quá 50 ký tự")]
        public string RoleName { get; set; } = string.Empty;

        [MaxLength(200, ErrorMessage = "Mô tả không được quá 200 ký tự")]
        public string? Description { get; set; }
    }
}
