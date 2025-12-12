using System;
using System.Collections.Generic;

namespace logistic_web.infrastructure.Models;

public partial class UserRole
{
    public int UserId { get; set; }

    public int RoleId { get; set; }

    public int? ShipperId { get; set; }

    public string? Description { get; set; }

    public virtual Role Role { get; set; } = null!;

    public virtual Shipper? Shipper { get; set; }

    public virtual User User { get; set; } = null!;
}
