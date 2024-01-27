using System;
using System.Collections.Generic;

namespace TradingApp.Models;

public partial class Item
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CraetedOn { get; set; }

    public int? EditedBy { get; set; }

    public DateTime? EditedOn { get; set; }

    public int? SaleUnit { get; set; }

    public virtual MeasureUnit IdNavigation { get; set; } = null!;

    public virtual ICollection<TradingDocumentDetail> TradingDocumentDetails { get; set; } = new List<TradingDocumentDetail>();
}
