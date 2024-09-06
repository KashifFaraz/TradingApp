using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TradingApp.DTOs;

namespace TradingApp.Models.Extension
{
    public static class ModelExtension
    {

        public static IQueryable<T> ApplyFilters<T>(this IQueryable<T> source, PaginationFilter filters)
        {
            if (filters == null || filters.filters == null || !filters.filters.Any())
                return source;

            var parameter = Expression.Parameter(typeof(T), "x");
            Expression combined = null;

            foreach (var filter in filters.filters)
            {
                if (string.IsNullOrWhiteSpace(filter.PropertyName) || string.IsNullOrEmpty(filter.Value?.ToString()))
                    continue; // Skip invalid filters

                var property = Expression.Property(parameter, filter.PropertyName);
                var propertyType = property.Type;

                // Handle nullable DateTime types without truncating the time part
                var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
                object filterValue = null;

                try
                {
                    filterValue = Convert.ChangeType(filter.Value, underlyingType);
                }
                catch (Exception ex)
                {
                    continue; // Skip if conversion fails
                }

                var constant = Expression.Constant(filterValue, propertyType);

                // Handle different filter operations
                Expression comparison = filter.Operation switch
                {
                    "Equals" => Expression.Equal(property, constant),
                    "Contains" when property.Type == typeof(string) => Expression.Call(property, "Contains", Type.EmptyTypes, constant),
                    "GreaterThan" => Expression.GreaterThan(property, constant),
                    "LessThan" => Expression.LessThan(property, constant),
                    _ => null
                };

                if (comparison != null)
                {
                    combined = combined == null ? comparison : Expression.AndAlso(combined, comparison);
                }
            }

            if (combined == null)
                return source;

            var lambda = Expression.Lambda<Func<T, bool>>(combined, parameter);
            return source.Where(lambda);
        }


    }
}
