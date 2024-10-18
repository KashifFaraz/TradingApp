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

        public int? ItemId { get; set; }
        [Required]
        [Display(Name = "Unit Price")]
        [DisplayFormat(DataFormatString = "{0:N}")]
        public decimal? UnitPrice { get; set; }
        [Display(Name = "Amount")]
        [DisplayFormat(DataFormatString = "{0:N}")]
        public decimal? Amount { get; set; }
        [Required]
        public int? Quantity { get; set; }

        [Display(Name = "Discount Percentage")]

        [Range(0,100)]
        public  decimal? DiscountPercentage { get; set; }

        [JsonIgnore]
        public virtual Invoice? Master { get; set; }


    }
}
