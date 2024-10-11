namespace TradingApp.ViewModels
{
    public class TableColumn
    {
        public string DisplayName { get; set; } // Column display name
        public string ClassName { get; set; }   // Column class for toggling visibility
        public bool IsVisible { get; set; }     // Visibility toggle
    }

}
