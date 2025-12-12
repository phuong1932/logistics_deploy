using System.ComponentModel.DataAnnotations;

namespace LogisticsWebApp.DTO
{
    public class UserRegisterModel
    {
        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        [MinLength(3, ErrorMessage = "Tên đăng nhập phải có ít nhất 3 ký tự")]
        [MaxLength(50, ErrorMessage = "Tên đăng nhập không được quá 50 ký tự")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ và tên là bắt buộc")]
        [MaxLength(100, ErrorMessage = "Họ và tên không được quá 100 ký tự")]
        public string FullName { get; set; } = string.Empty;

        public int RoleId { get; set; } = 0; // 0 = role mặc định, backend sẽ tự động gán role "user"
        public int? ShipperId { get; set; } = null;
    }

    public class RegisterResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }
}

