using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace TradingApp.Utility
{
    public static class Extensions
    {
        public static SelectList ToSelectList<TEnum>(this TEnum enumObj) where TEnum : struct, IComparable, IFormattable, IConvertible
        {
            var values = Enum.GetValues(typeof(TEnum)).Cast<TEnum>().Select(e => new SelectListItem
            {
                Text = e.GetDisplayName(),
                Value = Convert.ToInt32(e).ToString()
            }).ToList();

            return new SelectList(values, "Value", "Text");
        }

        public static string GetDisplayName<TEnum>(this TEnum enumValue)// where TEnum : struct, IComparable, IFormattable, IConvertible
        {
            var field = enumValue.GetType().GetField(enumValue.ToString());
            var attribute = field.GetCustomAttribute<DisplayAttribute>();

            return attribute == null ? enumValue.ToString() : attribute.Name;
        }
    }
}
