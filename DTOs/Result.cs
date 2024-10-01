namespace TradingApp.DTOs
{
    public class Result
    {
        public bool Success { get; }
        public string Message { get; }

        public Result(bool success, string message)
        {
            Success = success;
            Message = message;
        }
    }

}
