using System.ComponentModel.DataAnnotations;

namespace LogisticsWebApp.DTO
{
    public class UserLoginVM
    {
        [Required(ErrorMessage = "Username is required")]
        public string UsernameOrEmail { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
    }
}