namespace TradingApp.DTOs
{
    public class FilterCriteria
    {
        public required string PropertyName { get; set; }
        public required string Operation { get; set; }
        public required object Value { get; set; }

        public object GetTypedValue(Type targetType)
        {
            if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
            {
                if (DateTime.TryParse(Value.ToString(), out var dateValue))
                {
                    return dateValue;
                }
            }
            else if (targetType == typeof(int) || targetType == typeof(int?))
            {
                if (int.TryParse(Value.ToString(), out var intValue))
                {
                    return intValue;
                }
            }
            // Add other type conversions as needed

            return Value;
        }
    }


}
