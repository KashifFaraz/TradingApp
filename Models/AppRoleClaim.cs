using System;
using System.Collections.Generic;

namespace TradingApp.Models;

public partial class AppRoleClaim
{
    public int Id { get; set; }

    public string RoleId { get; set; } = null!;

    public string? ClaimType { get; set; }

    public string? ClaimValue { get; set; }

    public virtual AppRole Role { get; set; } = null!;
}
