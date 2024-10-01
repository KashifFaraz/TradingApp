using System.ComponentModel.DataAnnotations;
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

        public enum PaymentReconciliationStatus : byte
        {
            [Display(Name = "Not Reconciled")]
            Unreconciled = 0,

            [Display(Name = "Partial Reconciled")]
            PartialReconciled = 1,

            [Display(Name = "Reconciled")]
            Reconciled = 2
        }

        public enum DocumentStatus : byte
        {
            [Display(Name = "Draft")]
            Draft = 0,

            [Display(Name = "Finalized")]
            Finalized = 1,

            [Display(Name = "Partial Reconciled")]
            FinalizedPartialReconciled = 2,

            [Display(Name = "Reconciled")]
            FinalizedReconciled = 3,

        }

        public enum PaymentMethod
        {

            [Display(Name = "Cash")]
            Cash = 0,

            [Display(Name = "Bank Transfer")]
            BankTransfer = 1,

            [Display(Name = "Cheque")]
            Cheque = 2,

            [Display(Name = "Money Order")]
            MoneyOrder = 3,

        }
        public enum TransectionType
        {

            Invoice = 0,
            Reciept = 1,

        }
        public enum FinanceAccountType
        {
            Income = 0,
            Expense = 1,
            Asset = 2,
            Liability = 3,
            Equity = 4
        }
        public enum ComputationType
        {
            [Display(Name = "Fixed Amount")]
            FixedAmount = 0,
            [Display(Name = "Percentage")]
            Percentage = 1,

        }

        public static int CurrentOrganizationId = 1;
        public static int GetOrganizationId()
        {

            return CurrentOrganizationId += 1;


        }
    }
}
