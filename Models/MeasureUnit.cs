using System;
using System.Collections.Generic;

namespace TradingApp.Models;

public partial class MeasureUnit
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Symbol { get; set; }

    public virtual ICollection<Item> Items { get; set; } = new List<Item>();
}
