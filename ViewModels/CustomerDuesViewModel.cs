using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TradingApp.Models;

public class CustomerDuesViewModel
{
    public Stakeholder Stakeholder { get; set; }
    [Display(Name = "Total Receivable")]
    [DisplayFormat(DataFormatString = "{0:N}")]
    public decimal TotalReceivable { get; set; }
    public IEnumerable<Invoice> Invoices { get; set; }
    [Display(Name = "Unreconciled Payment")]
    [DisplayFormat(DataFormatString = "{0:N}")]
    public decimal UnreconciledPaymentAmount { get; set; }
    
}
