using System;
using System.Collections.Generic;

namespace TradingApp.Models;

public partial class ChartOfAccount
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int AccountTypeId { get; set; }

    public int? ParentAccountId { get; set; }

    public bool? IsTransactionAccount { get; set; }

    public virtual AccountType AccountType { get; set; } = null!;

    public virtual ICollection<ChartOfAccount> InverseParentAccount { get; set; } = new List<ChartOfAccount>();

    public virtual ChartOfAccount? ParentAccount { get; set; }
}
