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
        public static string SubString(string str, int length)
        {
            return SubString(str, 0, length);
        }

        #endregion

        #region 移除前导/后导字符串

        /// <summary>
        /// 移除前导字符串
        /// </summary>
        public static string TrimStart(string str, string trimStr, bool ignoreCase = true)
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
        public static string TrimEnd(string str, string trimStr, bool ignoreCase = true)
        {
            str = ConvertHelper.GetString(str);
            trimStr = ConvertHelper.GetString(trimStr);

            while (str.EndsWith(trimStr, ignoreCase, CultureInfo.CurrentCulture))
            {
                str = str.Substring(0, str.Length - trimStr.Length);
            }

            return str;
        }

        /// <summary>
        /// 移除前导和后导字符串
        /// </summary>
        public static string Trim(string str, string trimStr = " ", bool ignoreCase = true)
        {
            return TrimStart(TrimEnd(str, trimStr, ignoreCase), trimStr, ignoreCase);
        }

        #endregion
    }
}
