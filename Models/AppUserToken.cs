using System;
using System.Collections.Generic;

namespace TradingApp.Models;

public partial class AppUserToken
{
    public int UserId { get; set; }

    public string LoginProvider { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Value { get; set; }

    public virtual AppUser User { get; set; } = null!;
}
