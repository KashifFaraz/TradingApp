namespace TradingApp.Models
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }
        public bool? IsActive { get; set; } = true;
        public int? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? EditedBy { get; set; }
        public DateTime? EditedOn { get; set; }
    }

}
