using Microsoft.AspNetCore.Mvc.ModelBinding;
using TradingApp.DTOs;

namespace TradingApp.Filter
{
    public class PaginationFilterModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var query = bindingContext.HttpContext.Request.Query;

            var paginationFilter = new PaginationFilter
            {
                PageNumber = int.TryParse(query["PageNumber"], out var pageNumber) ? pageNumber : 1,
                PageSize = int.TryParse(query["PageSize"], out var pageSize) ? pageSize : 10,
                filters = new List<FilterCriteria>()
            };

            // Find the unique filter indexes based on the keys
            var filterIndexes = query.Keys
                .Where(k => k.StartsWith("filters["))
                .Select(k => int.Parse(k.Split(new[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries)[1]))
                .Distinct();

            // Now iterate over each unique filter index
            foreach (var index in filterIndexes)
            {
                var filterProperty = query[$"filters[{index}].PropertyName"].ToString();
                var filterOperation = query[$"filters[{index}].Operation"].ToString();
                var filterValue = query[$"filters[{index}].Value"].ToString();

                paginationFilter.filters.Add(new FilterCriteria
                {
                    PropertyName = filterProperty,
                    Operation = filterOperation,
                    Value = filterValue
                });
            }

            bindingContext.Result = ModelBindingResult.Success(paginationFilter);
            return Task.CompletedTask;
        }
    }


}
