using Microsoft.AspNetCore.Mvc.Rendering;
using Silmoon.Extension;
using System;
using System.Collections.Generic;

namespace Silmoon.AspNetCore
{
    public class MvcHtmlHelper
    {
        [Obsolete]
        public static SelectListItem[] GetSelectListItems(Enum selectedValue)
        {
            if (selectedValue.GetType().IsEnum)
            {
                var values = Enum.GetValues(selectedValue.GetType());
                var result = new SelectListItem[values.Length];

                for (int i = 0; i < values.Length; i++)
                {
                    var value = values.GetValue(i);
                    var item = new SelectListItem()
                    {
                        Text = ((Enum)value).GetDisplayName(),
                        Value = value.ToString(),
                        Selected = selectedValue.Equals(value)
                    };
                    result[i] = item;
                }
                return result;
            }
            else
            {
                return [];
            }
        }
    }
}
