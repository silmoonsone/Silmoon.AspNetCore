using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using MongoDB.Bson;
using System;
using System.Threading.Tasks;

namespace Silmoon.AspNetCore.Extensions.Binders
{
    public class ObjectIdBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.ModelType == typeof(ObjectId))
                return new BinderTypeModelBinder(typeof(ObjectIdBinder));
            else if (context.Metadata.ModelType == typeof(ObjectId?))
                return new BinderTypeModelBinder(typeof(ObjectIdBinder));
            else
                return null;
        }
    }
    public class ObjectIdBinder : IModelBinder
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

            bindingContext.Result = ModelBindingResult.Success(ObjectId.Parse(value));
            return Task.CompletedTask;
        }
    }
}