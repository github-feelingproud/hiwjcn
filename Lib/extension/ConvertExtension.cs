using System;
using System.Collections.Generic;
using System.Linq;
using Lib.helper;
using Lib.core;
using System.Collections.Specialized;

namespace Lib.extension
{
    public static class ConvertExtension
    {
        /// <summary>
        /// 转换为整型
        /// </summary>
        /// <param name="data">数据</param>
        public static int ToInt(this object data)
        {
            return ConvertHelper.GetInt(data.ToString());
        }

        /// <summary>
        /// 转换为双精度浮点数
        /// </summary>
        /// <param name="data">数据</param>
        public static double ToDouble(this object data)
        {
            return ConvertHelper.GetDouble(data.ToString());
        }

        /// <summary>
        /// 转换为双精度浮点数,并按指定的小数位4舍5入
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="digits">小数位数</param>
        public static double ToDouble(this object data, int digits)
        {
            return Math.Round(ToDouble(data), digits);
        }

        /// <summary>
        /// 转换为高精度浮点数
        /// </summary>
        /// <param name="data">数据</param>
        public static decimal ToDecimal(this object data)
        {
            return ConvertHelper.GetDecimal(data);
        }

        /// <summary>
        /// 转换为高精度浮点数,并按指定的小数位4舍5入
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="digits">小数位数</param>
        public static decimal ToDecimal(this object data, int digits)
        {
            return Math.Round(ToDecimal(data), digits);
        }

        /// <summary>
        /// 转换为日期
        /// </summary>
        /// <param name="data">数据</param>
        public static DateTime ToDate(this object data)
        {
            return ConvertHelper.GetDateTime(data, DateTime.Now);
        }

        /// <summary>
        /// 转换为布尔值
        /// </summary>
        /// <param name="data">数据</param>
        public static bool ToBool(this object data)
        {
            var list = new string[] { "1", "true", "yes", "on", "success", "t" };
            return list.Contains(data.ToString().ToLower().Trim());
        }

        /// <summary>
        /// true为1，false为0
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int ToBoolInt(this object data)
        {
            return data.ToBool() ? 1 : 0;
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
        /// 转为json 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string ToJson(this object data)
        {
            return JsonHelper.ObjectToJson(data);
        }

        /// <summary>
        /// json转为实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T JsonToEntity<T>(this string json)
        {
            return JsonHelper.JsonToEntity<T>(json);
        }

        /// <summary>
        /// 映射
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static T MapTo<T>(this object data)
        {
            return MapperHelper.GetMappedEntity<T>(data);
        }
        
        /// <summary>
        /// NameValueCollection转为字典
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ToDict(this NameValueCollection col)
        {
            var dict = new Dictionary<string, string>();
            foreach (var key in col.AllKeys)
            {
                dict[key] = col[key];
            }
            return dict;
        }

        /// <summary>
        /// 格式化数字，获取xxx xxxk xxxw
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string SimpleNumber(this Int64 num)
        {
            return Com.SimpleNumber(num);
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

    }
}
