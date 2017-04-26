using Lib.helper;
using Lib.ioc;
using System;
using Lib.extension;

namespace Lib.cache
{
    /// <summary>
    /// CacheManager 的摘要说明
    /// </summary>
    public static class CacheManager
    {
        public static ICacheProvider CacheProvider()
        {
            if (!AppContext.IsRegistered<ICacheProvider>())
            {
                return new MemoryCacheProvider();
            }
            return AppContext.GetObject<ICacheProvider>();
        }

        /// <summary>
        /// 如果使用缓存：如果缓存中有，就直接取。如果没有就先获取并加入缓存
        /// 如果不使用缓存：直接从数据源取。
        /// </summary>
        /// <returns></returns>
        public static T Cache<T>(string key, Func<T> dataSource, bool UseCache = true, double expires_minutes = 3)
        {
            if (!ValidateHelper.IsPlumpString(key)) { throw new Exception("缓存key为空"); }
            if (dataSource == null) { throw new Exception("缓存数据源为空"); }
            //如果读缓存，读到就返回
            if (UseCache)
            {
                var cacheManager = CacheProvider();
                try
                {
                    var cache = cacheManager.Get<T>(key);
                    if (cache.Success)
                    {
                        return cache.Result;
                    }
                    var data = dataSource.Invoke();
                    cacheManager.Set(key, data, (int)(expires_minutes * 60));
                    return data;
                }
                catch (Exception e)
                {
                    try
                    {
                        //读取缓存失败就尝试移除缓存
                        cacheManager.Remove(key);
                    }
                    catch (Exception ex)
                    {
                        ex.AddLog(typeof(CacheManager));
                    }
                    e.AddLog(typeof(CacheManager));
                }
            }
            return dataSource.Invoke();
        }

        public static void RemoveCache(string key)
        {
            var cacheManager = CacheProvider();
            cacheManager.Remove(key);
        }

        public static void RemoveByPattern(string pattern)
        {
            var cacheManager = CacheProvider();
            cacheManager.RemoveByPattern(pattern);
        }

        public static void ClearCache()
        {
            var cacheManager = CacheProvider();
            cacheManager.Clear();
        }

    }
}