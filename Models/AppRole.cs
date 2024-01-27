using System;
using System.Collections.Generic;

namespace TradingApp.Models;

public partial class AppRole
{
    public string Id { get; set; } = null!;

    public string? Name { get; set; }

    public string? NormalizedName { get; set; }

    public string? ConcurrencyStamp { get; set; }

    public virtual ICollection<AppRoleClaim> AppRoleClaims { get; set; } = new List<AppRoleClaim>();

    public virtual ICollection<AppUser> Users { get; set; } = new List<AppUser>();
}
