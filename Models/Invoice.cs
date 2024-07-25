﻿using System;
using System.Collections.Generic;

namespace TradingApp.Models;

public partial class Invoice
{
    public int Id { get; set; }

    public int? TransactionId { get; set; }

    public string? CustomId { get; set; }

    public DateTime? DocDate { get; set; }

    public int? StakeholderId { get; set; }

    public string? BankName { get; set; }

    public string? AccountTitle { get; set; }

    public int? Rfqid { get; set; }

    public DateTime? DueDate { get; set; }

    public string? Description { get; set; }

    public int? QuoteId { get; set; }

    public int? PurchaseOrderId { get; set; }

    public int? SalesOderId { get; set; }

    public string? Currency { get; set; }

    public decimal? SubTotal { get; set; }

    public decimal? TotalAmount { get; set; }

    public byte? PaymentType { get; set; }

    public byte? PaymentMethod { get; set; }

    public decimal? UnreconciledAmount { get; set; }

    public byte? PaymentReconciliationStatus { get; set; }

    public int? OrganizationId { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public int? EditedBy { get; set; }

    public DateTime? EditedOn { get; set; }

    public virtual AppUser? CreatedByNavigation { get; set; }

    public virtual AppUser? EditedByNavigation { get; set; }

    public virtual ICollection<Invoice> InversePurchaseOrder { get; set; } = new List<Invoice>();

    public virtual ICollection<Invoice> InverseQuote { get; set; } = new List<Invoice>();

    public virtual ICollection<Invoice> InverseRfq { get; set; } = new List<Invoice>();

    public virtual ICollection<Invoice> InverseSalesOder { get; set; } = new List<Invoice>();

    public virtual ICollection<InvoiceLine> InvoiceLines { get; set; } = new List<InvoiceLine>();

    public virtual Organization? Organization { get; set; }

    public virtual ICollection<PaymentReconciliation> PaymentReconciliations { get; set; } = new List<PaymentReconciliation>();

    public virtual Invoice? PurchaseOrder { get; set; }

    public virtual Invoice? Quote { get; set; }

    public virtual Invoice? Rfq { get; set; }

    public virtual Invoice? SalesOder { get; set; }

    public virtual Stakeholder? Stakeholder { get; set; }

    public virtual Transaction? Transaction { get; set; }
}
