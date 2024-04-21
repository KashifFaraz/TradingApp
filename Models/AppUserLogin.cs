using System;
using System.Collections.Generic;

namespace TradingApp.Models;

public partial class AppUserLogin
{
    public string LoginProvider { get; set; } = null!;

    public string ProviderKey { get; set; } = null!;

    public string? ProviderDisplayName { get; set; }

    public int UserId { get; set; }

    public virtual AppUser User { get; set; } = null!;
}
