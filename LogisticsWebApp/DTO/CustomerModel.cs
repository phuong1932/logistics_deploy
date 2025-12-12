using System.ComponentModel.DataAnnotations;

namespace LogisticsWebApp.DTO
{
    public class CustomerModel
    {
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Tên khách hàng là bắt buộc")]
        [StringLength(255, MinimumLength = 2, ErrorMessage = "Tên khách hàng phải từ 2 đến 255 ký tự")]
        public string CustomerName { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email phải đúng định dạng")]
        public string? Email { get; set; }

        [RegularExpression(@"^[0-9]{10,11}$", ErrorMessage = "Số điện thoại phải có 10-11 chữ số")]
        public string? Phone { get; set; }
        [Required(ErrorMessage = "Dịa chỉ là bắt buộc")]

        [StringLength(500, ErrorMessage = "Địa chỉ không được vượt quá 500 ký tự")]
        public string? Address { get; set; }
        [Required(ErrorMessage = "Người liên hệ là bắt buộc")]

        [StringLength(255, ErrorMessage = "Tên người liên hệ không được vượt quá 255 ký tự")]
        public string? PersonInCharge { get; set; }

        public bool IsSelected { get; set; } = false;
    }
}

