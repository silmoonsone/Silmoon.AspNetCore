using Microsoft.AspNetCore.Mvc.Rendering;
using Silmoon.Extension;
using System;
using System.Collections.Generic;

namespace Silmoon.AspNetCore
{
    public class MvcHtmlHelper
    {
        public static IEnumerable<SelectListItem> GetSelectListItems(Enum value)
        {
            if (value.GetType().IsEnum)
            {
                var values = Enum.GetValues(value.GetType());
                var result = new List<SelectListItem>();
                for (int i = 0; i < values.Length; i++)
                {
                    var item = new SelectListItem() { Text = ((Enum)values.GetValue(i)).GetDisplayName(), Value = values.GetValue(i).ToString() };
                    if (item.Value == values.GetValue(i).ToString())
                    {
                        item.Selected = true;
                    }
                    result.Add(item);
                }
                return result;
            }
            else
            {
                return new List<SelectListItem>();
            }
        }
    }
}
