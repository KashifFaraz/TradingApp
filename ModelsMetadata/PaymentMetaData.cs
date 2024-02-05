using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TradingApp.Models;

namespace TradingApp.Models
{
    [ModelMetadataType(typeof(PaymentMetadata))]
    public partial class Payment
    {
        [NotMapped]
        public int InvoiceId { get; set; }
    }

    public class PaymentMetadata
    { }
}