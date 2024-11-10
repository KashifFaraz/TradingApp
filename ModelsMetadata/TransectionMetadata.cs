using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TradingApp.Attribute;
using TradingApp.Models;
using TradingApp.Utility;

namespace TradingApp.Models
{
    [ModelMetadataType(typeof(TransectionMetadata))]
    public partial class Invoice: BaseEntity
    {
        //public Constants.PaymentType PaymentType { get; set; }
        //[NotMapped]
        // public string? CustomId { get; set; }

        [NotMapped]
        public decimal TotalPaid { get; set; }
        [NotMapped]
        public byte? DocStatusInput { get; set; }

    }


    public class TransectionMetadata
    {
        public int Id { get; set; }
        [Display(Name = "Custom Id")]
        public string? CustomId { get; set; }
        [Required]
        [Display(Name = "Doc. Date")]
        [DisplayFormat(DataFormatString = "{0:d}", NullDisplayText = "")]
        public DateTime? DocDate { get; set; }
        [Required]
        [Display(Name = "Customer")]
        public int? StakeholderId { get; set; }
        [Display(Name = "Bank Name")]
        public string? BankName { get; set; }
        [Display(Name = "Account Title")]
        public string? AccountTitle { get; set; }
        [Display(Name = "RFQ")]
        public int? Rfqid { get; set; }
        [Display(Name = "Due Date")]
        [DateGreaterThan("DocDate", ErrorMessage = "Due Date must be greater than Document Date.")]
        [DisplayFormat(DataFormatString = "{0:d}", NullDisplayText = "")]
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
        [DisplayFormat(DataFormatString = "{0:N}")]
        public decimal? SubTotal { get; set; }
        [Display(Name = "Total Amount")]
        [DisplayFormat(DataFormatString = "{0:N}")]
        public decimal? TotalAmount { get; set; }
        [Display(Name = "Tax Amount")]
        [DisplayFormat(DataFormatString = "{0:N}")]
        public decimal? TaxAmount { get; set; }
        [Display(Name = "Total Paid")]
        [DisplayFormat(DataFormatString = "{0:N}")]
        public decimal TotalPaid { get; set; }
        [Display(Name = "Unpaid Amount")]
        [DisplayFormat(DataFormatString = "{0:N}")]
        public decimal UnreconciledAmount { get; set; }
        public Constants.PaymentType PaymentType { get; set; }

        [Display(Name = "Created By")]
        public int? CreatedBy { get; set; }
        [Display(Name = "Created On")]
        public DateTime? CreatedOn { get; set; }
        [Display(Name = "Last Edited By")]
        public int? EditedBy { get; set; }
        [Display(Name = "Last Edited On")]
        public DateTime? EditedOn { get; set; }
        [Display(Name = "Payment Status")]
        public byte? PaymentReconciliationStatus { get; set; }

        [Display(Name = "Status")]
        public byte? DocStatus { get; set; }

        [StringLength(1000, ErrorMessage = "Terms cannot exceed 1000 characters.")]
        public string? Terms { get; set; }
        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters.")]
        public string? Notes { get; set; }


       

        [JsonIgnore]
        public virtual ICollection<Invoice> InverseSalesOder { get; set; } = new List<Invoice>();
        [Required]
        [AtLeastOneItem]
        public virtual ICollection<InvoiceLine> InvoiceLines { get; set; } = new List<InvoiceLine>();
        [Display(Name = "Customer")]
        public virtual Stakeholder? Stakeholder { get; set; }






    }
}
