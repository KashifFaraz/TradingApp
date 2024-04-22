using TradingApp.Models;

namespace TradingApp.Utility
{
    public static class Constants
    {
        public static readonly Dictionary<string, string> Currencies = new Dictionary<string, string>()
        {
            { "USD", "United States Dollar" },
            { "EUR", "Euro" },
            { "GBP", "British Pound Sterling" },
            { "JPY", "Japanese Yen" },
            { "CNY", "Chinese Yuan" },
            { "CAD", "Canadian Dollar" },
            { "AUD", "Australian Dollar" },
            { "CHF", "Swiss Franc" },
            { "PKR", "Pakistani Rupee" },
            { "INR", "Indian Rupee" }

        };

        public enum PaymentType
        {
            Inflow = 0,
            Outflow = 1

        }


        public enum PaymentMethod
        {

            Cash = 0,
            BankTransfer = 1,
            Cheque=2,
            MoneyOrder =3,

        }
        public enum TransectionType
        {

            Invoice = 0,
            Reciept = 1,

        }

        public static int CurrentOrganizationId = 1;
        public static int GetOrganizationId() {

            return CurrentOrganizationId += 1;


        }
    }
}
