using System;
using System.Collections.Generic;

namespace logistic_web.infrastructure.Models;

public partial class Tracking
{
    public long Id { get; set; }

    public string Username { get; set; } = null!;

    public string Action { get; set; } = null!;

    public DateTime DateCreated { get; set; }

    public string Ip { get; set; } = null!;
}
