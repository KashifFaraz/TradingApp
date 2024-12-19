using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TradingApp.Models;

namespace TradingApp.Models
{

    [ModelMetadataType(typeof(ItemMetaData))]
    public partial class Item
    {
       
    }

    public class ItemMetaData
    {
        [Display(Name = "Media Assets")]
        public string? ProductMediaAssets { get; set; }
        [Display(Name = "Sale Unit")]
        public int? SaleUnit { get; set; }
    }
}
