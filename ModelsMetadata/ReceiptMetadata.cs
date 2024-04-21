using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using TradingApp.Models;
using TradingApp.Utility;

namespace TradingApp.Models
{
    [ModelMetadataType(typeof(ReceiptMetadata))]
    public partial class Receipt
    {
        //public Constants.PaymentType PaymentType { get; set; }

    }

    public class ReceiptMetadata
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
        public Constants.PaymentType PaymentType { get; set; }

        [Display(Name = "Created By")]
        public int? CreatedBy { get; set; }
        [Display(Name = "Created On")]
        public DateTime? CreatedOn { get; set; }
        [Display(Name = "Last Edited By")]
        public int? EditedBy { get; set; }
        [Display(Name = "Last Edited On")]
        public DateTime? EditedOn { get; set; }

        [JsonIgnore]
        public virtual ICollection<Invoice> InverseInvoice { get; set; } = new List<Invoice>();

        [JsonIgnore]
        public virtual ICollection<Invoice> InversePurchaseOrder { get; set; } = new List<Invoice>();

        [JsonIgnore]
        public virtual ICollection<Invoice> InverseQuote { get; set; } = new List<Invoice>();

        [JsonIgnore]
        public virtual ICollection<Invoice> InverseRfq { get; set; } = new List<Invoice>();

        [JsonIgnore]
        public virtual ICollection<Invoice> InverseSalesOder { get; set; } = new List<Invoice>();
        
        [Display(Name = "Customer")]
        public virtual Stakeholder? Stakeholder { get; set; }




    }
}
