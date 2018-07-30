﻿using Lib.core;
using Lib.helper;
using System;
using System.Collections.Generic;

namespace Lib.extension
{
    public static class DictExtension
    {
        public static Value GetOrSet<Key, Value>(this IDictionary<Key, Value> dict, Key k, Func<Value> v)
        {
            if (!dict.ContainsKey(k))
            {
                dict[k] = v.Invoke();
            }
            return dict[k];
        }

        /// <summary>
        /// 字典变url格式(a=1&b=3)
        /// </summary>
        public static string ToUrlParam(this IDictionary<string, string> dict)
        {
            return Com.DictToUrlParams(dict);
        }

        /// <summary>
        /// 把一个字典加入另一个字典，重复就覆盖
        /// </summary>
        public static Dictionary<K, V> AddDict<K, V>(this Dictionary<K, V> dict, Dictionary<K, V> data)
        {
            foreach (var kv in data)
            {
                dict[kv.Key] = kv.Value;
            }
            return dict;
        }

        /// <summary>
        /// 计算签名，只是返回，不自动加入字典
        /// </summary>
        public static (string sign, string sign_data) GetSign(this Dictionary<string, string> param, string salt = null, string sign_req_key = "sign")
        {
            var sorted = SignHelper.FilterAndSort(param, sign_req_key, new MyStringComparer());
            var sign = SignHelper.CreateSign(sorted, salt);
            return (sign.sign, sign.sign_data);
        }

        /// <summary>
        /// 获取值
        /// </summary>
        public static V GetValueOrDefault<K, V>(this Dictionary<K, V> dict, K key, V deft = default(V))
        {
            if (dict.ContainsKey(key))
            {
                return dict[key];
            }
            return deft;
        }

        /// <summary>
        /// 返回元祖
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="dict"></param>
        /// <returns></returns>
        public static IEnumerable<(K key, V value)> AsTupleEnumerable<K, V>(this IDictionary<K, V> dict)
        {
            foreach (var key in dict.Keys)
            {
                yield return (key, dict[key]);
            }
        }

        static void fasd()
        {
            var dict = new Dictionary<string, string>();

            foreach (var kv in dict.AsTupleEnumerable())
            {
                //
            }
        }
    }
}
