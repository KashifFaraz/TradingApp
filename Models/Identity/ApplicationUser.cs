using Microsoft.AspNetCore.Identity;

namespace TradingApp.Models;

public class ApplicationUser : IdentityUser<int>
{
    public int? DefaultOrganization { get; set; }

}
