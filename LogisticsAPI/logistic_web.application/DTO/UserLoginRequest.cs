using System.ComponentModel.DataAnnotations;

namespace logistic_web.application.DTO
{
    public class UserLoginRequest
    {
        [Required(ErrorMessage = "Username hoặc Email là bắt buộc")]
        public string UsernameOrEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string Password { get; set; } = string.Empty;
    }
}
