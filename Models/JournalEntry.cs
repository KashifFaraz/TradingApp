using System;
using System.Collections.Generic;

namespace TradingApp.Models;

public partial class JournalEntry
{
    public int Id { get; set; }

    public byte TransactionType { get; set; }

    public int AccountId { get; set; }

    public int TransactionId { get; set; }

    public int? TransactionLineId { get; set; }

    public string Type { get; set; } = null!;

    public decimal Amount { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public int? EditedBy { get; set; }

    public DateTime? EditedOn { get; set; }
}
