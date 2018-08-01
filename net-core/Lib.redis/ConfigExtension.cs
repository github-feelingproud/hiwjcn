using Lib.cache;
using Lib.distributed;
using Lib.distributed.redis;
using Lib.ioc;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;

namespace Lib.redis
{
    public static class ConfigExtension
    {
        public static IServiceCollection UseRedis(this IServiceCollection collection, string connection_string,
            Action<IConnectionMultiplexer> config = null)
        {
            Func<IConnectionMultiplexer> source = () =>
            {
                var pool = ConnectionMultiplexer.Connect(connection_string);
                if (config != null)
                {
                    config.Invoke(pool);
                }
                /*
                pool.ConnectionFailed += (sender, e) => { e.Exception?.AddErrorLog("Redis连接失败:" + e.FailureType); };
                pool.ConnectionRestored += (sender, e) => { "Redis连接恢复".AddBusinessInfoLog(); };
                pool.ErrorMessage += (sender, e) => { e.Message?.AddErrorLog("Redis-ErrorMessage"); };
                pool.InternalError += (sender, e) => { e.Exception?.AddErrorLog("Redis内部错误"); };*/
                return pool;
            };
            collection.AddSingleton<IServiceWrapper<IConnectionMultiplexer>>(new RedisClientWrapper(string.Empty, source));
            collection.AddComponentDisposer<RedisComponentDisposer>();
            return collection;
        }

        /// <summary>
        /// 单例
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static IServiceCollection UseRedisCache(this IServiceCollection collection, Func<RedisCacheProviderConfig> config) =>
            collection.AddSingleton(config).AddSingleton<ICacheProvider, RedisCacheProvider_>();

        /// <summary>
        /// 瞬时对象
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static IServiceCollection UseRedisDistributedLock(this IServiceCollection collection, Func<RedisDistributedLockConfig> config) =>
            collection.AddSingleton(config).AddTransient<IDistributedLock, RedisDistributedLock>();

        public static IServiceCollection UseRedisFireWall(this IServiceCollection collection) => 
            throw new NotImplementedException();
    }
}
