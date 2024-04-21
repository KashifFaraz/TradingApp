using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using TradingApp.Models;

namespace TradingApp.Models
{
    [ModelMetadataType(typeof(TradingDocumentDetailMetaData))]
    public partial class InvoiceLine
    {
    }
    public class TradingDocumentDetailMetaData
    {

        [Required]
        public int? ItemId { get; set; }
        [Required]
        public decimal? UnitPrice { get; set; }
        [Required]
        public int? Quantity { get; set; }

        [JsonIgnore]
        public virtual Invoice? Master { get; set; }


    }
}
