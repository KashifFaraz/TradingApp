using System;
using System.Collections.Generic;

namespace TradingApp.Models;

public partial class Transaction
{
    public int TransactionId { get; set; }

    public string? CustomId { get; set; }

    public byte Type { get; set; }

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
