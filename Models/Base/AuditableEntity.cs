namespace TradingApp.Models
{
    public abstract class AuditableEntity
    {
        public bool? IsActive { get; set; } = true;
        public int? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? EditedBy { get; set; }
        public DateTime? EditedOn { get; set; }
    }

}
