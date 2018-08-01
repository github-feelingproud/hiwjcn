using Lib.core;
using Lib.ioc;
using StackExchange.Redis;

namespace Lib.redis
{
    public class RedisComponentDisposer : IDisposeComponent
    {
        private readonly IServiceWrapper<IConnectionMultiplexer> _wrapper;
        public RedisComponentDisposer(IServiceWrapper<IConnectionMultiplexer> wrapper) =>
            this._wrapper = wrapper;

        public string ComponentName => "redis组件";

        public int DisposeOrder => 1;

        public void Dispose()
        {
            this._wrapper.Value?.Dispose();
        }
    }
}
