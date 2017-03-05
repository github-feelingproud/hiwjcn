using System;
using System.Collections.Generic;
using System.Linq;
using Lib.helper;
using Lib.core;

namespace Lib.extension
{
    public static class ConvertExtension
    {
        /// <summary>
        /// 字典变url格式
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>
        public static string ToUrlParam(this IDictionary<string, string> dict)
        {
            return Com.DictToUrlParams(dict.ToDictionary(x => x.Key, x => x.Value));
        }

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
            return new string[] { "1", "true", "yes", "on" }.Contains(data.ToString().ToLower().Trim());
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
    }
}
