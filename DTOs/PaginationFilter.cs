namespace TradingApp.DTOs
{
    public class PaginationFilter
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public List<FilterCriteria> filters { get; set; }
    }
}
