

using System.ComponentModel.DataAnnotations;

namespace LogisticsWebApp.DTO
{
    public class ShipperModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên tài xế không được để trống")]
        [MinLength(2, ErrorMessage = "Tên tài xế phải có ít nhất 2 ký tự")]
        [MaxLength(100, ErrorMessage = "Tên tài xế không được quá 100 ký tự")]
        public string? TenTaiXe { get; set; }
        
        [Required(ErrorMessage = "Vui lòng chọn loại xe")]
        [Range(0, 2, ErrorMessage = "Chọn loại xe thích hợp")]
        public byte? LoaiXe { get; set; }
        [Required(ErrorMessage = "Số điện thoại không để trống")]
        [MaxLength(20, ErrorMessage = "Số điện thoại không được quá 20 ký tự")]
        [RegularExpression(@"^[0-9\-\+\(\)\s]*$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? SoDienThoai { get; set; }

        [MaxLength(500, ErrorMessage = "Địa chỉ không được quá 500 ký tự")]
        public string? DiaChi { get; set; }
    }
}

