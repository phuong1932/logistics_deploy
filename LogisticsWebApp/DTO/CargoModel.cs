namespace LogisticsWebApp.DTO
{
    public class CargoModel
    {
        public int Id { get; set; }
        public string? CargoCode { get; set; }
        public string CustomerCompanyName { get; set; } = string.Empty;
        public string EmployeeCreate { get; set; } = string.Empty;
        public string CustomerPersonInCharge { get; set; } = string.Empty;
        public string? CustomerAddress { get; set; }
        public byte? ServiceType { get; set; }
        public DateTime? LicenseDate { get; set; }
        public DateTime? ExchangeDate { get; set; }
        public decimal? EstimatedTotalAmount { get; set; }
        public decimal? AdvanceMoney { get; set; }
        public decimal? ShippingFee { get; set; }
        public int IdShipper { get; set; }
        public string? NameShipper { get; set; }
        public DateTime? CreatedAt { get; set; }
        public byte? StatusCargo { get; set; }
        public string? FilePath { get; set; }
        public bool IsSelected { get; set; }
    }

    public class ApiResponse
    {
        public bool Success { get; set; }
        public List<CargoModel>? Data { get; set; }
        public string? Message { get; set; }
    }
}
