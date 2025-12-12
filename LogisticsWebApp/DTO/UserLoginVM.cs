using System.ComponentModel.DataAnnotations;

namespace LogisticsWebApp.DTO
{
    public class UserLoginVM
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập hoặc email")]
        public string UsernameOrEmail { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        public string Password { get; set; } = string.Empty;
    }
}