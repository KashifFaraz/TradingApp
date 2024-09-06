using Microsoft.AspNetCore.Mvc.ModelBinding;
using TradingApp.DTOs;

namespace TradingApp.Filter
{
    public class BinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Metadata.ModelType == typeof(PaginationFilter))
            {
                return new PaginationFilterModelBinder();
            }

            return null;
        }
    }
}
