using System;
using System.Collections.Generic;

namespace TradingApp.Models;

public partial class AppUserClaim
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public string? ClaimType { get; set; }

    public string? ClaimValue { get; set; }

    public virtual AppUser User { get; set; } = null!;
}
