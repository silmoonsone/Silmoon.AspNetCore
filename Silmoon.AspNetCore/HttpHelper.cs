using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.IO;
using System.Net;
using System.Collections.Specialized;
using Silmoon.Extension;
using Microsoft.AspNetCore.Http;

namespace Silmoon.AspNetCore
{
    public class HttpHelper
    {
        public static string UrlPathEncode(string str, Encoding e)
        {
            if (str == null)
            {
                return null;
            }
            int index = str.IndexOf('?');
            if (index >= 0)
            {
                return (UrlPathEncode(str.Substring(0, index), e) + str.Substring(index));
            }
            return UrlEncodeSpaces(UrlEncodeNonAscii(str, e));
        }
        /// <summary>
        /// 获取应用程序相对路径，含有根路径“/”，若根路径不是应用程序路径，最后不是以斜杠“/”结束，如/apps/myapp。
        /// </summary>

        public static string MakeNewQueryString(NameValueCollection collection, string additionQueryString = "")
        {
            collection = new NameValueCollection(collection);
            string s = "";
            var tp = HttpUtility.ParseQueryString(additionQueryString);

            for (int i = 0; i < tp.Count; i++)
            {
                string key = tp.GetKey(i);
                string value = tp[i];

                collection[key] = value;
            }

            for (int i = 0; i < collection.Count; i++)
            {
                if (collection.GetKey(i) == null) continue;
                string key = collection.GetKey(i);
                string value = collection[i];

                s += $"{key}={value}&";
            }
            if (s != "")
            {
                s = s.Remove(s.Length - 1);
            }
            //if (s[0] != '?')
            //    s = "?" + s;

            return s;
        }
        public static string MakeQueryString(NameValueCollection parameters)
        {
            string result = string.Empty;
            for (int i = 0; i < parameters.Count; i++)
            {
                result += "&" + HttpUtility.UrlEncode(parameters.GetKey(i)) + "=" + HttpUtility.UrlEncode(parameters[i]);
            }
            return result.Substring(1, result.Length - 1);
        }



        static string UrlEncodeSpaces(string str)
        {
            if ((str != null) && (str.IndexOf(' ') >= 0))
            {
                str = str.Replace(" ", "%20");
            }
            return str;
        }
        static string UrlEncodeNonAscii(string str, Encoding e)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }
            if (e == null)
            {
                e = Encoding.UTF8;
            }
            byte[] bytes = e.GetBytes(str);
            bytes = UrlEncodeBytesToBytesInternalNonAscii(bytes, 0, bytes.Length, false);
            return Encoding.ASCII.GetString(bytes);
        }
        static byte[] UrlEncodeBytesToBytesInternalNonAscii(byte[] bytes, int offset, int count, bool alwaysCreateReturnValue)
        {
            int num = 0;
            for (int i = 0; i < count; i++)
            {
                if (IsNonAsciiByte(bytes[offset + i]))
                {
                    num++;
                }
            }
            if (!alwaysCreateReturnValue && (num == 0))
            {
                return bytes;
            }
            byte[] buffer = new byte[count + (num * 2)];
            int num3 = 0;
            for (int j = 0; j < count; j++)
            {
                byte b = bytes[offset + j];
                if (IsNonAsciiByte(b))
                {
                    buffer[num3++] = 0x25;
                    buffer[num3++] = (byte)IntToHex((b >> 4) & 15);
                    buffer[num3++] = (byte)IntToHex(b & 15);
                }
                else
                {
                    buffer[num3++] = b;
                }
            }
            return buffer;
        }
        static bool IsNonAsciiByte(byte b)
        {
            if (b < 0x7f)
            {
                return (b < 0x20);
            }
            return true;
        }
        static char IntToHex(int n)
        {
            if (n <= 9)
            {
                return (char)(n + 0x30);
            }
            return (char)((n - 10) + 0x61);
        }

    }
}
