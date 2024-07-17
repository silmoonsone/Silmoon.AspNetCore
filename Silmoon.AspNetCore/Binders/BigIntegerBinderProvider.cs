using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using System;

namespace Silmoon.AspNetCore.Binders
{
    public class BigIntegerBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.ModelType == typeof(BigInteger))
            {
                return new BinderTypeModelBinder(typeof(BigIntegerBinder));
            }
            else if (context.Metadata.ModelType == typeof(BigInteger?))
            {
                return new BinderTypeModelBinder(typeof(BigIntegerBinder));
            }
            else if (context.Metadata.ModelType == typeof(BigInteger[]))
            {
                return new BinderTypeModelBinder(typeof(BigIntegerArrayBinder));
            }
            else
                return null;
        }
    }
    public class BigIntegerBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var modelName = bindingContext.ModelName;

            // Try to fetch the value of the argument by name
            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

            if (valueProviderResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

            var value = valueProviderResult.FirstValue;

            // Check if the argument value is null or empty
            if (string.IsNullOrEmpty(value))
            {
                return Task.CompletedTask;
            }
            BigInteger.TryParse(value, out BigInteger result);
            bindingContext.Result = ModelBindingResult.Success(result);
            return Task.CompletedTask;
        }
    }
    public class BigIntegerArrayBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var modelName = bindingContext.ModelName;

            // Try to fetch the value of the argument by name
            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

            if (valueProviderResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

            var values = valueProviderResult.Values;

            List<BigInteger> list = new List<BigInteger>();

            foreach (var item in values)
            {
                if (string.IsNullOrEmpty(item)) continue;
                BigInteger.TryParse(item, out BigInteger result);
                list.Add(result);
            }

            bindingContext.Result = ModelBindingResult.Success(list.ToArray());
            return Task.CompletedTask;
        }
    }
}
