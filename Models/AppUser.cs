using System;
using System.Collections.Generic;

namespace TradingApp.Models;

public partial class AppUser
{
    public string Id { get; set; } = null!;

    public string? UserName { get; set; }

    public string? NormalizedUserName { get; set; }

    public string? Email { get; set; }

    public string? NormalizedEmail { get; set; }

    public bool EmailConfirmed { get; set; }

    public string? PasswordHash { get; set; }

    public string? SecurityStamp { get; set; }

    public string? ConcurrencyStamp { get; set; }

    public string? PhoneNumber { get; set; }

    public bool PhoneNumberConfirmed { get; set; }

    public bool TwoFactorEnabled { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }

    public bool LockoutEnabled { get; set; }

    public int AccessFailedCount { get; set; }

    public virtual ICollection<AppUserClaim> AppUserClaims { get; set; } = new List<AppUserClaim>();

    public virtual ICollection<AppUserLogin> AppUserLogins { get; set; } = new List<AppUserLogin>();

    public virtual ICollection<AppUserToken> AppUserTokens { get; set; } = new List<AppUserToken>();

    public virtual ICollection<AppRole> Roles { get; set; } = new List<AppRole>();
}
