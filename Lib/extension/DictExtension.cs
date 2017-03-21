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
        /// <param name="_locker"></param>
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
                        dict[key] = ins;
                    }
                }
            }
            return dict[key];
        }
    }
}
