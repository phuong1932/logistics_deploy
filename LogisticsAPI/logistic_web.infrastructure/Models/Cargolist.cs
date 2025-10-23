using System;
using System.Collections.Generic;

namespace logistic_web.infrastructure.Models;

public partial class Cargolist
{
    public int Id { get; set; }

    public string? CargoCode { get; set; }

    public string CustomerPersonInCharge { get; set; } = null!;

    public string CustomerCompanyName { get; set; } = null!;

    public string EmployeeCreate { get; set; } = null!;

    public string? CustomerAddress { get; set; }

    public byte? ServiceType { get; set; }

    public DateTime? LicenseDate { get; set; }

    public DateTime? ExchangeDate { get; set; }

    public decimal? EstimatedTotalAmount { get; set; }

    public decimal? AdvanceMoney { get; set; }

    public decimal? ShippingFee { get; set; }

    public int QuantityOfShipper { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? FilePathJson { get; set; }
}
