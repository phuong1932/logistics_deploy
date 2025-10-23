using System;
using System.ComponentModel.DataAnnotations;

namespace LogisticsWebApp.DTO
{
    public class CreateCargoModel
    {
        public string? CargoCode { get; set; }

        [Required(ErrorMessage = "Tên công ty khách hàng không được để trống")]
        public string CustomerCompanyName { get; set; } = string.Empty;

        public string EmployeeCreate { get; set; } = string.Empty;

        // Không required vì có thể để trống trong edit mode
        public string CustomerPersonInCharge { get; set; } = string.Empty;

        public string? CustomerAddress { get; set; }

        public string ServiceType { get; set; } = "";

        public DateTime? ExchangeDate { get; set; } = DateTime.Now;

        public DateTime? LicenseDate { get; set; }

        public string? NameOfLocation { get; set; }

        public decimal? EstimatedTotalAmount { get; set; }

        public decimal? AdvanceMoney { get; set; }

        public decimal? ShippingFee { get; set; }

        public int QuantityOfShipper { get; set; } = 1;
    }
}

