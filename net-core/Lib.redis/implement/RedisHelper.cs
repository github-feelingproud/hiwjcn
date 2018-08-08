using Lib.data;
using Lib.redis;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Lib.distributed.redis
{
    /// <summary>
    /// Redis操作
    /// https://github.com/qq1206676756/RedisHelp
    /// 个人觉得对NoSql数据库的操作不要使用异步，因为本身响应就很快，不会阻塞
    /// </summary>
    public partial class RedisHelper : SerializeBase,
        IRedisAll, IDisposable
    {
        private readonly int _db;
        private readonly IConnectionMultiplexer _conn;

        public IDatabase Database => this._conn.SelectDatabase(this._db);
        public IConnectionMultiplexer Connection => this._conn;

        public RedisHelper(IConnectionMultiplexer conn, int db)
        {
            this._conn = conn;
            this._db = db;
        }

        public const string DeleteKeyWithValueScript =
            @"if redis.call('get', KEYS[1]) == ARGV[1] then return redis.call('del',KEYS[1]) else return 0 end";

        /// <summary>
        /// key和value匹配的时候才删除
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public int DeleteKeyWithValue(string key, byte[] val) =>
            this.Do(db => (int)db.ScriptEvaluate(DeleteKeyWithValueScript, new RedisKey[] { key }, new RedisValue[] { val }));

        /// <summary>
        /// key和value匹配的时候才删除
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public async Task<int> DeleteKeyWithValueAsync(string key, byte[] val) =>
            await this.DoAsync(async db => (int)await db.ScriptEvaluateAsync(DeleteKeyWithValueScript, new RedisKey[] { key }, new RedisValue[] { val }));

        /// <summary>
        /// 如果key不存在时写入
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expire"></param>
        /// <returns></returns>
        public async Task<bool> StringSetWhenNotExist(string key, byte[] value, TimeSpan expire) =>
            await this.DoAsync(async db => await db.StringSetAsync(key, value, expire, When.NotExists));

        public T Do<T>(Func<IDatabase, T> func)
        {
            var database = _conn.GetDatabase(_db);
            return func(database);
        }

        public Task<T> DoAsync<T>(Func<IDatabase, Task<T>> func) =>
            this.Do(db => func.Invoke(db));

        public ITransaction CreateTransaction() => this.Database.CreateTransaction();

        public IServer GetServer(string hostAndPort)
        {
            return _conn.GetServer(hostAndPort);
        }

        public void Dispose()
        {
            //do nothing
        }
    }
}
