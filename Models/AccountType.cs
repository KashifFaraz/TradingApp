using System;
using System.Collections.Generic;

namespace TradingApp.Models;

public partial class AccountType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<ChartOfAccount> ChartOfAccounts { get; set; } = new List<ChartOfAccount>();
}
