using System;
using System.Collections.Generic;

namespace TradingApp.Models;

public partial class Tax
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public byte ComputationType { get; set; }
    public decimal Value { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? CreatedOn { get; set; }
    public int? EditedBy { get; set; }
    public DateTime? EditedOn { get; set; }

    
    
}
