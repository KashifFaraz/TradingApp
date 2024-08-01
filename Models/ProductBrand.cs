using System;
using System.Collections.Generic;

namespace TradingApp.Models;

public partial class ProductBrand
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public int? EditedBy { get; set; }

    public DateTime? EditedOn { get; set; }

    public virtual ICollection<Item> Items { get; set; } = new List<Item>();
}
