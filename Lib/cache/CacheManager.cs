using Lib.helper;
using Lib.ioc;
using System;
using Lib.extension;
using System.Threading.Tasks;

namespace Lib.cache
{
    /// <summary>
    /// CacheManager 的摘要说明
    /// </summary>
    public static class CacheManager
    {
        /// <summary>
        /// 获取缓存组件
        /// </summary>
        /// <returns></returns>
        public static ICacheProvider CacheProvider() => AppContext.GetObject<ICacheProvider>();

        /// <summary>
        /// 如果使用缓存：如果缓存中有，就直接取。如果没有就先获取并加入缓存
        /// 如果不使用缓存：直接从数据源取。
        /// </summary>
        public static T Cache<T>(string key, Func<T> dataSource,
            bool UseCache = true, double expires_minutes = 3)
        {
            //如果读缓存，读到就返回
            if (UseCache)
            {
                var cacheManager = CacheProvider();
                return cacheManager.GetOrSet(key, dataSource, TimeSpan.FromMinutes(expires_minutes));
            }
            return dataSource.Invoke();
        }

        /// <summary>
        /// 异步缓存
        /// </summary>
        public static async Task<T> CacheAsync<T>(string key, Func<Task<T>> dataSource,
            bool UseCache = true, double expires_minutes = 3)
        {
            //如果读缓存，读到就返回
            if (UseCache)
            {
                var cacheManager = CacheProvider();
                return await cacheManager.GetOrSetAsync(key, dataSource, TimeSpan.FromMinutes(expires_minutes));
            }
            return await dataSource.Invoke();
        }

        /// <summary>
        /// 删除缓存
        /// </summary>
        /// <param name="key"></param>
        public static void RemoveCache(string key)
        {
            var cacheManager = CacheProvider();
            cacheManager.Remove(key);
        }

        /// <summary>
        /// 通过正则删除缓存
        /// </summary>
        /// <param name="pattern"></param>
        public static void RemoveByPattern(string pattern)
        {
            var cacheManager = CacheProvider();
            cacheManager.RemoveByPattern(pattern);
        }

        /// <summary>
        /// 清空缓存
        /// </summary>
        public static void ClearCache()
        {
            var cacheManager = CacheProvider();
            cacheManager.Clear();
        }

    }
}