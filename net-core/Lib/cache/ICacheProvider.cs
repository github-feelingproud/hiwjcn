using System;
using System.Text;
using Newtonsoft.Json;
using Lib.data;
using System.Configuration;

namespace Lib.cache
{
    /// <summary>
    /// ���Ƿ�ɹ���־λ�Ľ��
    /// </summary>
    [Serializable]
    public class CacheResult<T>
    {
        public virtual bool Success { get; set; } = false;
        public virtual T Result { get; set; }
    }

    /// <summary>
    /// �������
    /// </summary>
    public abstract class CacheBase : SerializeBase
    {
        //
    }

    /// <summary>
    /// �����������
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
        [Obsolete("���Ƽ�ʹ�ã�Ч�ʵͣ�����provider�޷�ʵ��")]
        void RemoveByPattern(string pattern);

        /// <summary>
        /// Clear all cache data
        /// </summary>
        [Obsolete("���Ƽ�ʹ�ã�Ч�ʵͣ�����provider�޷�ʵ��")]
        void Clear();
    }
}
