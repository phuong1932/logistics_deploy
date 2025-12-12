namespace logistic_web.application.DTO
{
    /// <summary>
    /// DTO để tạo và cập nhật shipper
    /// </summary>
    public class ShipperDto
    {
        /// <summary>
        /// Tên tài xế
        /// </summary>
        public string? TenTaiXe { get; set; }

        /// <summary>
        /// Loại xe (0: Xe tải nhỏ, 1: Xe tải trung, 2: Xe tải lớn)
        /// </summary>
        public byte? LoaiXe { get; set; }

        /// <summary>
        /// Số điện thoại
        /// </summary>
        public string? SoDienThoai { get; set; }

        /// <summary>
        /// Địa chỉ
        /// </summary>
        public string? DiaChi { get; set; }
    }
}

