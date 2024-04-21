using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TradingApp.Models;

namespace TradingApp.Models
{
    [ModelMetadataType(typeof(OrganizationMetaData))]
    public partial class Organization
    {
        [NotMapped]
        [Required]
        public IFormFile ImageFile { get; set; }

    }

    public class OrganizationMetaData
    {
        [Required]
        public string? Name { get; set; }

        [Required]
        public string? DefaultCurrency { get; set; }
       
    }
}