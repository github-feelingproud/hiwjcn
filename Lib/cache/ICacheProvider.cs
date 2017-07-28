using System;
using System.Text;
using Newtonsoft.Json;
using Lib.data;
using System.Configuration;

namespace Lib.cache
{
    /// <summary>
    /// 带是否成功标志位的结果
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CacheResult<T>
    {
        public virtual bool Success { get; set; }
        public virtual T Result { get; set; }
    }

    /// <summary>
    /// 缓存基类
    /// </summary>
    public abstract class CacheBase : SerializeBase
    {
        protected readonly string CacheKeyPrefix = ConfigurationManager.AppSettings["CacheKeyPrefix"] ?? "Lib.Default.CacheKeyPrefix";

        protected string Wrapper(string key)
        {
            return $"{this.CacheKeyPrefix}.{key}";
        }

    }

    /// <summary>
    /// 缓存命中情况
    /// </summary>
    public enum CacheHitStatusEnum : int
    {
        NotHit = 0,
        Hit = 1
    }

    /// <summary>
    /// Cache manager interface
    /// </summary>
    public interface ICacheProvider : IDisposable
    {
        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>The value associated with the specified key.</returns>
        CacheResult<T> Get<T>(string key);

        /// <summary>
        /// Adds the specified key and object to the cache.
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="data">Data</param>
        /// <param name="expire">Cache time</param>
        void Set(string key, object data, TimeSpan expire);

        /// <summary>
        /// Gets a value indicating whether the value associated with the specified key is cached
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>Result</returns>
        bool IsSet(string key);

        /// <summary>
        /// Removes the value with the specified key from the cache
        /// </summary>
        /// <param name="key">/key</param>
        void Remove(string key);

        /// <summary>
        /// Removes items by pattern
        /// </summary>
        /// <param name="pattern">pattern</param>
        void RemoveByPattern(string pattern);

        /// <summary>
        /// Clear all cache data
        /// </summary>
        void Clear();
    }
}
