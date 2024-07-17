using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Dynamic;
using System.Text;
using System.Web;

namespace Silmoon.AspNetCore.Html
{
    public static class HtmlHelper
    {
        public static string MakeAMark(string text, string url, string target)
        {
            return "<a href=\"" + url + "\" target=\"" + target + "\">" + text + "</a>";
        }
        /// <summary>
        /// 返回已经设计好的A标签。
        /// </summary>
        /// <param name="text">显示的文字</param>
        /// <param name="url">导航的URL</param>
        /// <param name="target">目标</param>
        /// <param name="alt">Tip文字</param>
        /// <returns></returns>
        public static string MakeAMark(string text, string url, string target, string alt)
        {
            return "<a href=\"" + url + "\" target=\"" + target + "\" alt=\"" + alt + "\">" + text + "</a>";
        }
        /// <summary>
        /// 返回已经设计好的A标签。
        /// </summary>
        /// <param name="text">显示的文字</param>
        /// <param name="url">导航的URL</param>
        /// <returns></returns>
        public static string MakeAMark(string text, string url)
        {
            return "<a href=\"" + url + "\">" + text + "</a>";
        }
        public static string MakeImage(string url, int width, int height)
        {
            return "<img src=\"" + url + "\" width=\"" + width + "\" height=\"" + height + "\" />";
        }
        public static string MakeImage(string url)
        {
            return "<img src=\"" + url + "\" />";
        }
        /// <summary>
        /// 将Textarea中的HTML写入数据库的前置处理
        /// </summary>
        /// <param name="html">HTML</param>
        /// <returns></returns>
        public static string TextareaInHTML(string html)
        {
            html = html.Replace(" ", "&nbsp;");
            html = html.Replace("\r\n", "<br />");
            return html;
        }
        /// <summary>
        /// 将HTML回写到Textarea的前置处理
        /// </summary>
        /// <param name="html">HTML</param>
        /// <returns></returns>
        public static string TextareaOutHTML(string html)
        {
            html = html.Replace("&nbsp;", " ");
            html = html.Replace("<br />", "\r\n");
            return html;
        }

        public static string MakePostFormHtml(NameValueCollection values, string postTo, string name, bool submit = false)
        {
            string result = "<form method=\"post\" action=\"" + postTo + "\" name=\"" + name + "\" id=\"" + name + "\">\r\n";
            for (int i = 0; i < values.Count; i++)
            {
                result += "<input type='hidden' name='" + values.GetKey(i) + "' value='" + values[i] + "'/>\r\n";
            }
            result += "</form>\r\n";
            if (submit) result += "<script>document.forms['" + name + "'].submit();</script>";
            return result;
        }
    }
}