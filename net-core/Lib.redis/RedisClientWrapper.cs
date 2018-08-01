using Lib.ioc;
using StackExchange.Redis;
using System;

namespace Lib.redis
{
    public class RedisClientWrapper : LazyServiceWrapperBase<IConnectionMultiplexer>
    {
        public RedisClientWrapper(string name, Func<IConnectionMultiplexer> source) : base(name, source)
        {
            //
        }
    }
}
