using System;
using System.Collections.Generic;

namespace TradingApp.Models;

public partial class PaymentReconciliation
{
    public int Id { get; set; }

    public int? Paymentid { get; set; }

    public int? TradingDocumentId { get; set; }

    public decimal? Amount { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CraetedOn { get; set; }

    public int? EditedBy { get; set; }

    public DateTime? EditedOn { get; set; }

    public virtual Payment? Payment { get; set; }

    public virtual TradingDocument? TradingDocument { get; set; }
}
