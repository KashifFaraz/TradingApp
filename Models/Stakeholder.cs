using System;
using System.Collections.Generic;

namespace TradingApp.Models;

public partial class Stakeholder
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int? Type { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CraetedOn { get; set; }

    public int? EditedBy { get; set; }

    public DateTime? EditedOn { get; set; }
}
