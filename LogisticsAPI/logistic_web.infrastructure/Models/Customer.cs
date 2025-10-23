using System;
using System.Collections.Generic;

namespace logistic_web.infrastructure.Models;

public partial class Customer
{
    public int CustomerId { get; set; }

    public string CustomerName { get; set; } = null!;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string? PersonInCharge { get; set; }
}
