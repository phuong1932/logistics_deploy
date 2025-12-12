using System;
using System.Collections.Generic;

namespace logistic_web.infrastructure.Models;

public partial class Shipper
{
    public int Id { get; set; }

    public string? TenTaiXe { get; set; }

    public byte? LoaiXe { get; set; }

    public string? SoDienThoai { get; set; }

    public string? DiaChi { get; set; }

    public virtual ICollection<Cargolist> Cargolists { get; set; } = new List<Cargolist>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
