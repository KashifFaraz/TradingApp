using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TradingApp.Models;

namespace TradingApp.Models
{
    [ModelMetadataType(typeof(TradingDocuemntMetadata))]
    public partial class TradingDocument
    {
    }

    public class TradingDocuemntMetadata
    {
        public int Id { get; set; }
        [Display(Name = "Custom Id")]
        public string? CustomId { get; set; }
        [Required]
        [Display(Name = "Doc. Date")]
        [DisplayFormat(DataFormatString = "{0:d}", NullDisplayText = "")]
        public DateTime? DocDate { get; set; }
        [Display(Name = "Stakeholder")]
        public int? StakeholderId { get; set; }
        [Display(Name = "Bank Name")]
        public string? BankName { get; set; }
        [Display(Name = "Account Title")]
        public string? AccountTitle { get; set; }
        [Display(Name = "RFQ")]
        public int? Rfqid { get; set; }
        [Display(Name = "Due Date")]

        public DateTime? DueDate { get; set; }

        public string? Description { get; set; }
        [Display(Name = "Quote")]

        public int? QuoteId { get; set; }
        [Display(Name = "Purchase Order")]
        public int? PurchaseOrderId { get; set; }
        [Display(Name = "Sales Order")]
        public int? SalesOderId { get; set; }
        [Display(Name = "Invoice")]
        public int? InvoiceId { get; set; }
        [Display(Name = "Sub Total")]
        public decimal? SubTotal { get; set; }
        [Display(Name = "Total Amount")]
        public decimal? TotalAmount { get; set; }
        [Display(Name = "Created By")]
        public int? CreatedBy { get; set; }
        [Display(Name = "Craeted On")]
        public DateTime? CraetedOn { get; set; }
        [Display(Name = "Edited By")]
        public int? EditedBy { get; set; }
        [Display(Name = "Edited On")]
        public DateTime? EditedOn { get; set; }
    }
}
