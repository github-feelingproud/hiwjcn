using Lib.extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.cache
{
    public static class CacheExtension
    {
        /// <summary>
        /// 缓存逻辑
        /// </summary>
        public static T GetOrSet<T>(this ICacheProvider cacheManager,
            string key, Func<T> dataSource, TimeSpan expire, Action OnHit = null)
        {
            try
            {
                var cache = cacheManager.Get<T>(key);
                if (cache.Success)
                {
                    OnHit?.Invoke();

                    return cache.Result;
                }
                var data = dataSource.Invoke();
                cacheManager.Set(key, data, expire);
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
                //缓存错误
                return dataSource.Invoke();
            }
        }

        /// <summary>
        /// 异步缓存逻辑
        /// </summary>
        public static async Task<T> GetOrSetAsync<T>(this ICacheProvider cacheManager,
            string key, Func<Task<T>> dataSource, TimeSpan expire,
            Func<Task> OnHitAsync = null, Action OnHit = null)
        {
            try
            {
                var cache = cacheManager.Get<T>(key);
                if (cache.Success)
                {
                    var onhit_task = OnHitAsync?.Invoke();
                    if (onhit_task != null)
                    {
                        await onhit_task;
                    }
                    OnHit?.Invoke();

                    return cache.Result;
                }
                var data = await dataSource.Invoke();
                cacheManager.Set(key, data, expire);
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
                //缓存错误
                return await dataSource.Invoke();
            }
        }
    }
}
