﻿using System;
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

    public decimal? Quantity { get; set; }
    public decimal DiscountPercentage { get; set; }
    public  decimal? TaxPercentage { get; set; }
    public  decimal? TaxAmount { get; set; }
    public int SortOrder { get; set; }

    public virtual Item? Item { get; set; }
    
    public virtual Invoice? Master { get; set; }
}
