using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace Silmoon.AspNetCore.Extensions
{
    public class JArrayBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.ModelType == typeof(JArray))
                return new BinderTypeModelBinder(typeof(JArrayBinder));
            else return null;
        }
    }
    public class JArrayBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            ArgumentNullException.ThrowIfNull(bindingContext);
            var modelName = bindingContext.ModelName;
            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

            if (valueProviderResult == ValueProviderResult.None) return Task.CompletedTask;

            bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

            var value = valueProviderResult.FirstValue;
            if (string.IsNullOrEmpty(value)) return Task.CompletedTask;

            bindingContext.Result = ModelBindingResult.Success(JArray.Parse(value));
            return Task.CompletedTask;
        }
    }
}
