using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.helper;

namespace Lib.extension
{
    /// <summary>
    /// StringExtension
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// trim string
        /// </summary>
        /// <param name="str"></param>
        /// <param name="tm"></param>
        /// <returns></returns>
        public static string Trim_(this string str, string tm)
        {
            return StringHelper.Trim(str, tm);
        }

        /// <summary>
        /// 如果不是有效字符串就转换为null
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string EmptyAsNull(this string str) => ConvertHelper.EmptyAsNull(str);

        /// <summary>
        /// 找到#标签#
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static List<string> FindTags(this string s)
        {
            return Com.FindTagsFromStr(s);
        }

        /// <summary>
        /// 找到@的人
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static List<string> FindAt(this string s)
        {
            return Com.FindAtFromStr(s);
        }

        /// <summary>
        /// 获取sha1
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string ToSHA1(this string s)
        {
            return SecureHelper.GetSHA1(s);
        }

        /// <summary>
        /// 转为md5
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string ToMD5(this string data)
        {
            return SecureHelper.GetMD5(data);
        }

        /// <summary>
        /// 获取拼音
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string GetPinyin(this string s)
        {
            return Com.Pinyin(s);
        }

        /// <summary>
        /// 获取拼音首字母
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string GetSpell(this string s)
        {
            return Com.GetSpell(s);
        }

        /// <summary>
        /// 提取url后面的参数
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ExtractUrlParams(this string s)
        {
            return Com.ExtractUrlParams(s);
        }

        /// <summary>
        /// 模仿python的join
        /// </summary>
        /// <param name="sep"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string Join<T>(this string sep, IEnumerable<T> list) => sep.Join_(list);

        /// <summary>
        /// 模仿python的join
        /// </summary>
        /// <param name="sep"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string Join_<T>(this string sep, IEnumerable<T> list)
        {
            var arrs = list.Select(x => ConvertHelper.GetString(x)).ToArray();
            return string.Join(sep, arrs);
        }

        /// <summary>
        /// 模仿python中的格式化
        /// </summary>
        /// <param name="s"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string Format(this string s, params object[] args) => s.Format_(args);

        /// <summary>
        /// 模仿python中的格式化
        /// </summary>
        /// <param name="s"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string Format_(this string s, params object[] args) => string.Format(s, args);

        /// <summary>
        /// base64变string
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string Base64ToString(this string s) => ConvertHelper.Base64Decode(s);

        /// <summary>
        /// string变base64
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string StringToBase64(this string s) => ConvertHelper.Base64Encode(s);

        /// <summary>
        /// 字节数组到base64
        /// </summary>
        /// <param name="bs"></param>
        /// <returns></returns>
        public static string BytesToBase64(this byte[] bs) => ConvertHelper.BytesToBase64(bs);

        /// <summary>
        /// base64到字节数组
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static byte[] Base64ToBytes(this string s) => ConvertHelper.Base64ToBytes(s);
    }
}
