using Lib.core;
using Lib.ioc;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Lib.redis
{
    public static class ConfigExtension
    {
        public static void UseRedis(this IServiceCollection collection)
        {
            collection.AddComponentDisposer<RedisDispose>();
        }

        public static void UseRedisCache()
        { }
    }

    public class RedisDispose : IDisposeComponent
    {
        public string ComponentName => "redis组件";

        public int Order => 1;

        public void Dispose()
        {
            //
        }
    }
}
