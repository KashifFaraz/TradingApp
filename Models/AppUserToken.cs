using System;
using System.Collections.Generic;

namespace TradingApp.Models;

public partial class AppUserToken
{
    public string UserId { get; set; } = null!;

    public string LoginProvider { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Value { get; set; }

    public virtual AppUser User { get; set; } = null!;
}
