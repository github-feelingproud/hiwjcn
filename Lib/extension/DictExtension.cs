using Lib.helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.extension
{
    public static class DictExtension
    {
        /// <summary>
        /// 维护多个实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="func"></param>
        /// <param name="CheckInstance"></param>
        /// <param name="_locker">一定要是引用类型</param>
        /// <returns></returns>
        public static T StoreInstance<T>(this IDictionary<string, T> dict, string key, Func<T> func,
            Func<T, bool> CheckInstance = null, object _locker = null, Action<T> releaseInstance = null)
        {
            //如果为空就默认都可以
            if (CheckInstance == null) { CheckInstance = _ => true; }
            //如果没有指定locker就使用dict作为locker
            if (_locker == null) { _locker = dict; }

            if (dict.ContainsKey(key))
            {
                var ins = dict[key];
                if (CheckInstance(ins))
                {
                    return ins;
                }
                else
                {
                    releaseInstance?.Invoke(ins);
                    dict.Keys.Remove(key);
                }
            }
            if (!dict.ContainsKey(key))
            {
                lock (_locker)
                {
                    if (!dict.ContainsKey(key))
                    {
                        var ins = func();
                        if (!CheckInstance(ins)) { throw new Exception("新生成的实例没有通过验证"); }
                        dict[key] = ins;
                    }
                }
            }
            return dict[key];
        }

        /// <summary>
        /// 字典变url格式(a=1&b=3)
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>
        public static string ToUrlParam(this IDictionary<string, string> dict)
        {
            return Com.DictToUrlParams(dict.ToDictionary(x => x.Key, x => x.Value));
        }

        /// <summary>
        /// 把一个字典加入另一个字典，重复就覆盖
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="dict"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Dictionary<K, V> AddDict<K, V>(this Dictionary<K, V> dict, Dictionary<K, V> data)
        {
            foreach (var kv in data)
            {
                dict[kv.Key] = kv.Value;
            }
            return dict;
        }
    }
}
