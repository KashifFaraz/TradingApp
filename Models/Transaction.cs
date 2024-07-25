using System;
using System.Collections.Generic;

namespace TradingApp.Models;

public partial class Transaction
{
    public int TransactionId { get; set; }

    public byte Type { get; set; }

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual ICollection<Receipt> Receipts { get; set; } = new List<Receipt>();
}
