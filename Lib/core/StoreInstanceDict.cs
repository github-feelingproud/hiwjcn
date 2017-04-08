using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.extension;

namespace Lib.core
{
    /// <summary>
    /// 存储实例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class StoreInstanceDict<T> : Dictionary<string, T>
    {
        public readonly object _locker = new object();
    }

    /// <summary>
    /// 扩展
    /// </summary>
    public static class StoreInstanceDictExtension
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
        public static T StoreInstance<T>(this StoreInstanceDict<T> dict, string key, Func<T> func,
            Func<T, bool> CheckInstance = null, Action<T> releaseInstance = null)
        {
            //如果为空就默认都可以
            if (CheckInstance == null) { CheckInstance = _ => true; }

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
                    dict.Remove(key);
                }
            }
            if (!dict.ContainsKey(key))
            {
                lock (dict._locker)
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
    }

    /// <summary>
    /// 实例管理
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class StaticClientManager<T> : StaticClientManager<T, string>
    {
        public override string CreateStringKey(string key)
        {
            return key;
        }
    }

    /// <summary>
    /// T是缓存对象，K是创建T所需要的参数类型，缓存Key通过K去生成
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="K"></typeparam>
    public abstract class StaticClientManager<T, K> : IDisposable
    {
        protected readonly StoreInstanceDict<T> db = new StoreInstanceDict<T>();

        /// <summary>
        /// 创建string key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual string CreateStringKey(K key)
        {
            return key.ToJson().ToSHA1();
        }

        #region 等待实现
        /// <summary>
        /// 默认客户端
        /// </summary>
        public abstract T DefaultClient { get; }

        /// <summary>
        /// 创建对象
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public abstract T CreateNewClient(K key);

        /// <summary>
        /// 释放对象
        /// </summary>
        /// <param name="ins"></param>
        public abstract void DisposeBrokenClient(T ins);

        /// <summary>
        /// 检查对象
        /// </summary>
        /// <param name="ins"></param>
        /// <returns></returns>
        public abstract bool CheckClient(T ins);

        /// <summary>
        /// 释放所有对象
        /// </summary>
        public abstract void Dispose();
        #endregion

        /// <summary>
        /// 获取实例
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public T GetCachedClient(K key)
        {
            var str_key = CreateStringKey(key);
            var geter = new Func<T>(() => CreateNewClient(key));
            var check = new Func<T, bool>(CheckClient);
            var dispose = new Action<T>(DisposeBrokenClient);

            var ins = db.StoreInstance(str_key, geter, check, dispose);
            return ins;
        }
    }
}
