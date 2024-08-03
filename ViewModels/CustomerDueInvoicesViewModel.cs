using System.ComponentModel.DataAnnotations;

namespace TradingApp.ViewModels
{
    public class CustomerDueInvoicesViewModel
    {
        [Display(Name = "Customer Id")]
        public int StakeholderId { get; set; }
        [Display(Name = "Customer Name")]
        public string Name { get; set; }
        [Display(Name = "Unpaid Invoice")]
        public int UnpaidInvoice { get; set; }
        [Display(Name = "Total Invoice")]
        [DisplayFormat(DataFormatString = "{0:N}")]
        public decimal TotalAmount { get; set; }
    }

}
