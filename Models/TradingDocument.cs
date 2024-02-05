using System;
using System.Collections.Generic;

namespace TradingApp.Models;

public partial class TradingDocument
{
    public int Id { get; set; }

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

    public int? InvoiceId { get; set; }

    public decimal? SubTotal { get; set; }

    public decimal? TotalAmount { get; set; }

    public decimal? UnreconciledAmount { get; set; }

    public byte? PaymentReconciliationStatus { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CraetedOn { get; set; }

    public int? EditedBy { get; set; }

    public DateTime? EditedOn { get; set; }

    public virtual ICollection<TradingDocument> InverseInvoice { get; set; } = new List<TradingDocument>();

    public virtual ICollection<TradingDocument> InversePurchaseOrder { get; set; } = new List<TradingDocument>();

    public virtual ICollection<TradingDocument> InverseQuote { get; set; } = new List<TradingDocument>();

    public virtual ICollection<TradingDocument> InverseRfq { get; set; } = new List<TradingDocument>();

    public virtual ICollection<TradingDocument> InverseSalesOder { get; set; } = new List<TradingDocument>();

    public virtual TradingDocument? Invoice { get; set; }

    public virtual ICollection<PaymentReconciliation> PaymentReconciliations { get; set; } = new List<PaymentReconciliation>();

    public virtual TradingDocument? PurchaseOrder { get; set; }

    public virtual TradingDocument? Quote { get; set; }

    public virtual TradingDocument? Rfq { get; set; }

    public virtual TradingDocument? SalesOder { get; set; }

    public virtual Stakeholder? Stakeholder { get; set; }

    public virtual ICollection<TradingDocumentDetail> TradingDocumentDetails { get; set; } = new List<TradingDocumentDetail>();
}
