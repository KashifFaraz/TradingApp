using System.ComponentModel.DataAnnotations;

namespace TradingApp.Utility
{
    public class AtLeastOneItemAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is IEnumerable<object> list && list.Any())
            {
                return ValidationResult.Success;
            }

            return new ValidationResult("At least one item is required.");
        }
    }
}
