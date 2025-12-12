using System.ComponentModel.DataAnnotations;

namespace logistic_web.application.DTO
{
    public class CreateTrackingRequest
    {
        [Required(ErrorMessage = "Username là bắt buộc")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Action là bắt buộc")]
        public string Action { get; set; } = string.Empty;
    }
}

