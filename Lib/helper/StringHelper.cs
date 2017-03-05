using System;
using System.Text;
using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Lib.helper
{
    /// <summary>
    /// 字符串帮助类
    /// </summary>
    public static class StringHelper
    {
        /// <summary>
        /// join 不会返回空
        /// </summary>
        /// <param name="spliter"></param>
        /// <param name="list"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static string Join(string spliter, IList<string> list)
        {
            string str = string.Empty;
            if (!ValidateHelper.IsPlumpList(list)) { return str; }
            list.ToList().ForEach(delegate (string s)
            {
                s = ConvertHelper.GetString(s);
                str += ((str == string.Empty) ? string.Empty : spliter) + s;
            });
            return str;
        }

        /// <summary>
        /// 可能返回空
        /// </summary>
        /// <param name="str"></param>
        /// <param name="spliter"></param>
        /// <returns></returns>
        public static List<string> Split(string str, params char[] spliter)
        {
            return ConvertHelper.GetString(str).Split(spliter).ToList();
        }

        #region 截取字符串

        /// <summary>
        /// 截取字符串
        /// </summary>
        /// <param name="str">源字符串</param>
        /// <param name="startIndex">开始位置的索引</param>
        /// <param name="length">子字符串的长度</param>
        /// <returns></returns>
        public static string SubString(string str, int startIndex, int length)
        {
            str = ConvertHelper.GetString(str);
            if (str.Length > (startIndex + length))
            {
                return str.Substring(startIndex, length);
            }
            return str;
        }

        /// <summary>
        /// 截取字符串
        /// </summary>
        /// <param name="str">源字符串</param>
        /// <param name="length">子字符串的长度</param>
        /// <returns></returns>
        public static string SubString(string str, int length)
        {
            return SubString(str, 0, length);
        }

        #endregion

        #region 移除前导/后导字符串

        /// <summary>
        /// 移除前导字符串
        /// </summary>
        /// <param name="str">源字符串</param>
        /// <param name="trimStr">移除字符串</param>
        /// <returns></returns>
        public static string TrimStart(string str, string trimStr)
        {
            return TrimStart(str, trimStr, true);
        }

        /// <summary>
        /// 移除前导字符串
        /// </summary>
        /// <param name="str">源字符串</param>
        /// <param name="trimStr">移除字符串</param>
        /// <param name="ignoreCase">是否忽略大小写</param>
        /// <returns></returns>
        public static string TrimStart(string str, string trimStr, bool ignoreCase)
        {
            str = ConvertHelper.GetString(str);
            trimStr = ConvertHelper.GetString(trimStr);

            while (str.StartsWith(trimStr, ignoreCase, CultureInfo.CurrentCulture))
            {
                str = str.Remove(0, trimStr.Length);
            }

            return str;
        }

        /// <summary>
        /// 移除后导字符串
        /// </summary>
        /// <param name="sourceStr">源字符串</param>
        /// <param name="trimStr">移除字符串</param>
        /// <returns></returns>
        public static string TrimEnd(string sourceStr, string trimStr)
        {
            return TrimEnd(sourceStr, trimStr, true);
        }

        /// <summary>
        /// 移除后导字符串
        /// </summary>
        /// <param name="str">源字符串</param>
        /// <param name="trimStr">移除字符串</param>
        /// <param name="ignoreCase">是否忽略大小写</param>
        /// <returns></returns>
        public static string TrimEnd(string str, string trimStr, bool ignoreCase)
        {
            str = ConvertHelper.GetString(str);
            trimStr = ConvertHelper.GetString(trimStr);

            while (str.EndsWith(trimStr, ignoreCase, CultureInfo.CurrentCulture))
            {
                str = str.Substring(0, str.Length - trimStr.Length);
            }

            return str;
        }

        public static string Trim(string str)
        {
            return Trim(str, " ");
        }

        /// <summary>
        /// 移除前导和后导字符串
        /// </summary>
        /// <param name="sourceStr">源字符串</param>
        /// <param name="trimStr">移除字符串</param>
        /// <returns></returns>
        public static string Trim(string sourceStr, string trimStr)
        {
            return Trim(sourceStr, trimStr, true);
        }

        /// <summary>
        /// 移除前导和后导字符串
        /// </summary>
        /// <param name="sourceStr">源字符串</param>
        /// <param name="trimStr">移除字符串</param>
        /// <param name="ignoreCase">是否忽略大小写</param>
        /// <returns></returns>
        public static string Trim(string str, string trimStr, bool ignoreCase)
        {
            return TrimStart(TrimEnd(str, trimStr, ignoreCase), trimStr, ignoreCase);
        }

        #endregion
    }
}
