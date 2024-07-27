using System.ComponentModel.DataAnnotations;

namespace TradingApp.Utility
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            var attribute = enumValue.GetType()
                .GetField(enumValue.ToString())
                .GetCustomAttributes(typeof(DisplayAttribute), false)
                .FirstOrDefault() as DisplayAttribute;

            return attribute == null ? enumValue.ToString() : attribute.Name;
        }

        public static string GetEnumDisplayName<TEnum>(byte? statusValue) where TEnum : Enum
        {
            if (!statusValue.HasValue) return string.Empty;

            var statusEnum = (TEnum)Enum.ToObject(typeof(TEnum), statusValue.Value);
            return statusEnum.GetDisplayName();
        }
    }
}
