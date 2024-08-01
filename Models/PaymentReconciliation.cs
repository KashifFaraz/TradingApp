using System;
using System.Collections.Generic;

namespace TradingApp.Models;

public partial class PaymentReconciliation
{
    public int Id { get; set; }

    public int? PaymentId { get; set; }

    public int? TradingDocumentId { get; set; }

    public decimal? Amount { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CraetedOn { get; set; }

    public int? EditedBy { get; set; }

    public DateTime? EditedOn { get; set; }

    public virtual Invoice? TradingDocument { get; set; }
}
