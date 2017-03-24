using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.helper;

namespace Lib.extension
{
    public static class StringExtension
    {
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
    }
}
