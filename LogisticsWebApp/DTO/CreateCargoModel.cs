using System;
using System.ComponentModel.DataAnnotations;

namespace LogisticsWebApp.DTO
{
    public class CreateCargoModel
    {
        [StringLength(100, ErrorMessage = "Mã Cargo không được vượt quá 100 ký tự")]
        public string? CargoCode { get; set; }

        [Required(ErrorMessage = "Tên công ty khách hàng không được để trống")]
        [StringLength(255, ErrorMessage = "Tên công ty khách hàng không được vượt quá 255 ký tự")]
        public string CustomerCompanyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nhân viên tạo không được để trống")]
        [StringLength(255, ErrorMessage = "Tên nhân viên không được vượt quá 255 ký tự")]
        public string EmployeeCreate { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên người phụ trách không được để trống")]
        [StringLength(255, ErrorMessage = "Tên người phụ trách không được vượt quá 255 ký tự")]
        public string CustomerPersonInCharge { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nơi giao hàng không được để trống")]
        [StringLength(500, ErrorMessage = "Địa chỉ giao hàng không được vượt quá 500 ký tự")]
        public string? CustomerAddress { get; set; }

        [Required(ErrorMessage = "Loại dịch vụ không được để trống")]
        public string ServiceType { get; set; } = "";

        [Required(ErrorMessage = "Ngày giao dịch không được để trống")]
        public DateTime? ExchangeDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Ngày nhận chứng từ không được để trống")]
        public DateTime? LicenseDate { get; set; }

        // [Required(ErrorMessage = "Nơi giao hàng không được để trống")]
        // [StringLength(500, ErrorMessage = "Nơi giao hàng không được vượt quá 500 ký tự")]
        // public string? NameOfLocation { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Tổng tiền ước tính phải là số dương")]
        public decimal? EstimatedTotalAmount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Tiền ứng phải là số dương")]
        public decimal? AdvanceMoney { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Phí vận chuyển phải là số dương")]
        public decimal? ShippingFee { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn shipper hợp lệ")]
        public int IdShipper { get; set; }

        [Range(1, 5, ErrorMessage = "Trạng thái đơn hàng không hợp lệ")]
        public byte? StatusCargo { get; set; }
    }
}

