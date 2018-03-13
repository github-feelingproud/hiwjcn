using Polly;
using System;
using System.Threading.Tasks;

namespace Lib.distributed.redis
{
    /// <summary>
    /// Redis分布式锁
    /// https://github.com/KidFashion/redlock-cs
    /// </summary>
    public class RedisDistributedLock : IDistributedLock
    {
        private readonly int DB_NUM;
        private readonly string _key;
        private readonly byte[] _value;

        private readonly RedisHelper _redis;

        private readonly Policy _retryAsync = Policy.Handle<Exception>()
            .WaitAndRetryAsync(5, i => TimeSpan.FromMilliseconds(100 * 5));

        public RedisDistributedLock(string key, int db = 1) : this(null, key, db)
        { }

        public RedisDistributedLock(string connection_string, string key, int db)
        {
            this.DB_NUM = db;
            this._key = key ?? throw new ArgumentNullException(nameof(key));
            this._value = Guid.NewGuid().ToByteArray();

            this._redis = new RedisHelper(db, connection_string);
        }

        public async Task LockOrThrow()
        {
            await this._retryAsync.ExecuteAsync(async () =>
            {
                var success = await this._redis.StringSetWhenNotExist(this._key, this._value, TimeSpan.FromMinutes(10));
                if (!success) { throw new Exception("没有拿到redis锁，再试一次"); }
            });
        }

        public async Task ReleaseLock()
        {
            await this._redis.DeleteKeyWithValueAsync(this._key, this._value);
        }

        public void Dispose()
        {
            this._redis.Dispose();
        }

    }
}
