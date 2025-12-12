using System;

namespace logistic_web.api.DTO
{
    /// <summary>
    /// DTO cho việc trả về thông tin cargo từ API
    /// </summary>
    public class CargoResponse
    {
        /// <summary>
        /// ID của cargo
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Mã lô hàng
        /// </summary>
        public string? CargoCode { get; set; }

        /// <summary>
        /// Tên công ty khách hàng
        /// </summary>
        public string CustomerCompanyName { get; set; } = string.Empty;

        /// <summary>
        /// Tên nhân viên tạo lô hàng
        /// </summary>
        public string EmployeeCreate { get; set; } = string.Empty;

        /// <summary>
        /// Người phụ trách khách hàng
        /// </summary>
        public string CustomerPersonInCharge { get; set; } = string.Empty;

        /// <summary>
        /// Địa chỉ khách hàng
        /// </summary>
        public string? CustomerAddress { get; set; }

        /// <summary>
        /// Loại dịch vụ (0: XUAT, 1: NHAP, 2: XUAT NHAP)
        /// </summary>
        public byte? ServiceType { get; set; }

        /// <summary>
        /// Ngày nhận chứng từ
        /// </summary>
        public DateTime? LicenseDate { get; set; }

        /// <summary>
        /// Ngày giao dịch
        /// </summary>
        public DateTime? ExchangeDate { get; set; }

        /// <summary>
        /// Tổng số tiền ước tính cho lô hàng
        /// </summary>
        public decimal? EstimatedTotalAmount { get; set; }

        /// <summary>
        /// Số tiền tạm ứng cho lô hàng
        /// </summary>
        public decimal? AdvanceMoney { get; set; }

        /// <summary>
        /// Phí vận chuyển của lô hàng
        /// </summary>
        public decimal? ShippingFee { get; set; }

        /// <summary>
        /// ID Shipper (khóa ngoại tới bảng Shipper)
        /// </summary>
        public int IdShipper { get; set; }

        /// <summary>
        /// Trạng thái đơn hàng (1: Hoạt động, 2: Vô hiệu, 3: Chờ shipper nhận, 4: Đang vận chuyển, 5: Hoàn thành)
        /// </summary>
        public byte? StatusCargo { get; set; }

        /// <summary>
        /// Ngày tạo
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// Đường dẫn file cargo
        /// </summary>
        public string? FilePathJson { get; set; }
    }
}
