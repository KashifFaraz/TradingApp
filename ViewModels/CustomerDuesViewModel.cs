using System;
using System.Collections.Generic;

namespace TradingApp.Models;

public class CustomerDuesViewModel
{
    public Stakeholder Stakeholder { get; set; }
    public IEnumerable<Invoice> Invoices { get; set; }
}
