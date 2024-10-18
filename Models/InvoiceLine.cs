using System;
using System.Collections.Generic;

namespace TradingApp.Models;

public partial class InvoiceLine : BaseEntity
{
    public int Id { get; set; }

    public int? MasterId { get; set; }

    public string? Description { get; set; }

    public int? ItemId { get; set; }

    public decimal? UnitPrice { get; set; }

    public decimal? SubTotal { get; set; }
    public decimal? Amount { get; set; }

    public int? Quantity { get; set; }
    public virtual decimal? DiscountPercentage { get; set; }

    //public int? CreatedBy { get; set; }

    //public DateTime? CraetedOn { get; set; }

    //public int? EditedBy { get; set; }

    //public DateTime? EditedOn { get; set; }

    //public bool? IsActive { get; set; }

    public virtual Item? Item { get; set; }
    
    public virtual Invoice? Master { get; set; }
}
