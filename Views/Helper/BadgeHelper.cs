using static TradingApp.Utility.Constants;
using TradingApp.Utility;

namespace TradingApp.Views.Helper
{
    public static class BadgeHelper
    {
        public static (string BadgeClass, string StatusDisplayName, string DataUrl, string OnClickAttribute) GetBadgeDetails(byte? status, int itemId)
        {
            string badgeClass = "bg-secondary"; // Default to grey
            string statusDisplayName = "Unknown";
            string dataUrl = null;
            string onClickAttribute = null;

            if (status.HasValue)
            {
                statusDisplayName = EnumExtensions.GetEnumDisplayName<PaymentReconciliationStatus>(status.Value);

                switch ((PaymentReconciliationStatus)status.Value)
                {
                    case PaymentReconciliationStatus.Reconciled:
                        badgeClass = "bg-success"; // Green
                        break;
                    case PaymentReconciliationStatus.PartialReconciled:
                        badgeClass = "bg-warning text-dark"; // Yellow
                        dataUrl = $"Receipts/{itemId}/Reconcile";
                        onClickAttribute = "openDrawer(this)";
                        break;
                    case PaymentReconciliationStatus.Unreconciled:
                        badgeClass = "bg-danger"; // Red
                        dataUrl = $"Receipts/{itemId}/Reconcile";
                        onClickAttribute = "openDrawer(this)";
                        break;
                }
            }

            return (badgeClass, statusDisplayName, dataUrl, onClickAttribute);
        }
    }

}
