using System;
using System.Collections.Generic;

namespace TradingApp.Models;

public partial class Payment
{
    public int Id { get; set; }

    public string? CustomId { get; set; }

    public DateTime? DocDate { get; set; }

    public int? StakeholderId { get; set; }

    public string? BankName { get; set; }

    public string? AccountTitle { get; set; }

    public DateTime? DueDate { get; set; }

    public int? PaymentType { get; set; }

    public string? Description { get; set; }

    public decimal? SubTotal { get; set; }

    public decimal? TotalAmount { get; set; }

    public decimal? UnreconciledAmount { get; set; }

    public byte? PaymentReconciliationStatus { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CraetedOn { get; set; }

    public int? EditedBy { get; set; }

    public DateTime? EditedOn { get; set; }

    public virtual ICollection<PaymentReconciliation> PaymentReconciliations { get; set; } = new List<PaymentReconciliation>();
}
