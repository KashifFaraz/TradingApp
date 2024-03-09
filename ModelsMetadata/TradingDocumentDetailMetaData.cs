using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using TradingApp.Models;

namespace TradingApp.Models
{
    [ModelMetadataType(typeof(TradingDocumentDetailMetaData))]
    public partial class TradingDocumentDetail
    {
    }
    public class TradingDocumentDetailMetaData
    {
        [Required]
        public decimal? UnitPrice { get; set; }
        [JsonIgnore]
        public virtual TradingDocument? Master { get; set; }


    }
}
