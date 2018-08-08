using Lib.cache;
using Lib.distributed;
using Lib.distributed.redis;
using Lib.extension;
using Lib.helper;
using Lib.ioc;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;

namespace Lib.redis
{
    public static class RedisBootstrap
    {
        public static readonly string DefaultName = Com.GetUUID();

        public static IServiceCollection UseRedis(this IServiceCollection collection,
            string connection_string, Action<IConnectionMultiplexer> config = null)
        {
            Func<IConnectionMultiplexer> source = () =>
            {
                var pool = ConnectionMultiplexer.Connect(connection_string);
                if (config == null)
                {
                    config = (x) =>
                    {
                        x.ConnectionFailed += (sender, e) => e.Exception?.AddErrorLog("Redis连接失败:" + e.FailureType);
                        x.ConnectionRestored += (sender, e) => "Redis连接恢复".AddBusinessInfoLog();
                        x.ErrorMessage += (sender, e) => e.Message?.AddErrorLog("Redis-ErrorMessage");
                        x.InternalError += (sender, e) => e.Exception?.AddErrorLog("Redis内部错误");
                    };
                }
                config.Invoke(pool);
                return pool;
            };
            collection.AddSingleton<IServiceWrapper<IConnectionMultiplexer>>(new RedisClientWrapper(RedisBootstrap.DefaultName, source));

            //helpers
            collection.AddTransient<IRedis, RedisHelper>();
            collection.AddTransient<IRedisList, RedisHelper>();
            collection.AddTransient<IRedisString, RedisHelper>();
            collection.AddTransient<IRedisSet, RedisHelper>();
            collection.AddTransient<IRedisSortedSet, RedisHelper>();
            collection.AddTransient<IRedisPubSub, RedisHelper>();
            collection.AddTransient<IRedisKey, RedisHelper>();
            //all of them above
            collection.AddTransient<IRedisAll, RedisHelper>();

            //disposer
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
