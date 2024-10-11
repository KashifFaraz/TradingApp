namespace TradingApp.ViewModels
{
    public class TableViewModel<T>
    {
        public List<TableColumn> Columns { get; set; } // Dynamic column list
        public List<T> Data { get; set; }              // Generic data list
        public int TotalPages { get; set; }            // For pagination if needed
        public int PageIndex { get; set; }
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;
    }

}
