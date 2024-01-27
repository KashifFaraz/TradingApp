using System;
using System.Collections.Generic;

namespace TradingApp.Models;

public partial class StakeholderType
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<TradingDocument> TradingDocuments { get; set; } = new List<TradingDocument>();
}
