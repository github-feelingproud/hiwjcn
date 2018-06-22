using Lib.data;
using Lib.helper;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Lib.distributed.redis
{
    /// <summary>
    /// Redis操作
    /// https://github.com/qq1206676756/RedisHelp
    /// 个人觉得对NoSql数据库的操作不要使用异步，因为本身响应就很快，不会阻塞
    /// </summary>
    public class RedisHelper : SerializeBase, IDisposable
    {
        private readonly int DbNum;
        private readonly ConnectionMultiplexer _conn;

        public IDatabase Database => this._conn.SelectDatabase(this.DbNum);
        public IConnectionMultiplexer Connection => this._conn;

        public RedisHelper(int dbNum = 1) : this(dbNum, null) { }

        public RedisHelper(int dbNum, string readWriteHosts)
        {
            this.DbNum = dbNum;
            this._conn = 
                ValidateHelper.IsPlumpString(readWriteHosts) ? 
                RedisClientManager.Instance.GetCachedClient(readWriteHosts) : 
                RedisClientManager.Instance.DefaultClient;
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

        #region String

        /// <summary>
        /// 保存一个对象
        /// </summary>
        public bool StringSet(string key, object obj, TimeSpan? expiry = default(TimeSpan?))
        {
            return Do(db => db.StringSet(key, Serialize(obj), expiry));
        }

        /// <summary>
        /// 获取一个key的对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T StringGet<T>(string key)
        {
            return Do(db => Deserialize<T>(db.StringGet(key)));
        }

        /// <summary>
        /// 为数字增长val
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val">可以为负</param>
        /// <returns>增长后的值</returns>
        public double StringIncrement(string key, double val = 1)
        {
            return Do(db => db.StringIncrement(key, val));
        }

        /// <summary>
        /// 为数字减少val
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val">可以为负</param>
        /// <returns>减少后的值</returns>
        public double StringDecrement(string key, double val = 1)
        {
            return Do(db => db.StringDecrement(key, val));
        }
        #endregion String

        #region List

        /// <summary>
        /// 移除指定ListId的内部List的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void ListRemove(string key, object value)
        {
            Do(db => db.ListRemove(key, Serialize(value)));
        }

        /// <summary>
        /// 获取指定key的List
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<T> ListRange<T>(string key)
        {
            return Do(redis =>
            {
                var values = redis.ListRange(key);
                return values.Where(x => x.HasValue).Select(x => Deserialize<T>(x)).ToList();
            });
        }

        /// <summary>
        /// 入队
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void ListRightPush(string key, object value)
        {
            Do(db => db.ListRightPush(key, Serialize(value)));
        }

        /// <summary>
        /// 出队
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T ListRightPop<T>(string key)
        {
            return Do(db =>
            {
                var value = db.ListRightPop(key);
                return Deserialize<T>(value);
            });
        }

        /// <summary>
        /// pop或者阻塞
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="delay_ms"></param>
        /// <param name="timeout"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        private T PopOrBlock<T>(string key, int delay_ms, TimeSpan? timeout, Func<IDatabase, RedisValue> func)
        {
            return Do(db =>
            {
                var timeout_at = default(DateTime?);
                if (timeout != null)
                {
                    timeout_at = DateTime.Now.AddMilliseconds(timeout.Value.Milliseconds);
                }

                while (true)
                {
                    var value = func.Invoke(db);
                    if (!value.HasValue)
                    {
                        Thread.Sleep(delay_ms);
                        if (timeout_at != null && DateTime.Now > timeout_at.Value)
                        {
                            throw new Exception("等待超时");
                        }
                        continue;
                    }
                    return Deserialize<T>(value);
                }
            });
        }

        /// <summary>
        /// 拿不到数据就阻塞
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="delay_ms"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public T BListRightPop<T>(string key, int delay_ms = 10, TimeSpan? timeout = null)
        {
            return PopOrBlock<T>(key, delay_ms, timeout, db => db.ListRightPop(key));
        }

        /// <summary>
        /// 入栈
        /// </summary>
        public long ListLeftPush(string key, object value)
        {
            return Do(db => db.ListLeftPush(key, Serialize(value)));
        }

        /// <summary>
        /// 出栈
        /// </summary>
        public T ListLeftPop<T>(string key)
        {
            return Do(db =>
            {
                var value = db.ListLeftPop(key);
                return Deserialize<T>(value);
            });
        }

        /// <summary>
        /// 拿不到数据就阻塞
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="delay_ms"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public T BListLeftPop<T>(string key, int delay_ms = 10, TimeSpan? timeout = null)
        {
            return PopOrBlock<T>(key, delay_ms, timeout, db => db.ListLeftPop(key));
        }

        /// <summary>
        /// 获取集合中的数量
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public long ListLength(string key)
        {
            return Do(db => db.ListLength(key));
        }

        #endregion List

        #region Set
        public bool SetAdd(string key, object value) => Do(db => db.SetAdd(key, Serialize(value)));
        public List<T> SetMembers<T>(string key) => Do(db => db.SetMembers(key).Select(x => Deserialize<T>(x)).ToList());
        public bool SetRemove<T>(string key, object value) => Do(db => db.SetRemove(key, Serialize(value)));
        public List<T> SetCombine<T>(SetOperation operation, params string[] keys)
        {
            var redis_keys = keys.Select(x =>
            {
                RedisKey k = x;
                return k;
            }).ToArray();
            return Do(db => db.SetCombine(operation, redis_keys).Select(x => Deserialize<T>(x)).ToList());
        }
        #endregion

        #region SortedSet
        public bool SortedSetAdd(string key, object value, double score) => Do(db => db.SortedSetAdd(key, Serialize(value), score));
        public bool SortedSetRemove<T>(string key, object value) => Do(db => db.SortedSetRemove(key, Serialize(value)));
        #endregion

        #region Hash
        public bool HashSet(string key, string k, object value) => Do(db => db.HashSet(key, k, Serialize(value)));
        public bool HashDelete(string key, string k) => Do(db => db.HashDelete(key, k));
        #endregion

        #region key

        /// <summary>
        /// 删除单个key
        /// </summary>
        /// <param name="key">redis key</param>
        /// <returns>是否删除成功</returns>
        public bool KeyDelete(string key)
        {
            return Do(db => db.KeyDelete(key));
        }

        /// <summary>
        /// 判断key是否存储
        /// </summary>
        /// <param name="key">redis key</param>
        /// <returns></returns>
        public bool KeyExists(string key)
        {
            return Do(db => db.KeyExists(key));
        }

        /// <summary>
        /// 重新命名key
        /// </summary>
        /// <param name="key">就的redis key</param>
        /// <param name="newKey">新的redis key</param>
        /// <returns></returns>
        public bool KeyRename(string key, string newKey)
        {
            return Do(db => db.KeyRename(key, newKey));
        }

        /// <summary>
        /// 设置Key的时间
        /// </summary>
        /// <param name="key">redis key</param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public bool KeyExpire(string key, TimeSpan? expiry = default(TimeSpan?))
        {
            return Do(db => db.KeyExpire(key, expiry));
        }

        #endregion key

        #region 发布订阅

        /// <summary>
        /// Redis发布订阅  订阅
        /// </summary>
        /// <param name="subChannel"></param>
        /// <param name="handler"></param>
        public void Subscribe(string subChannel, Action<RedisChannel, RedisValue> handler = null)
        {
            var sub = _conn.GetSubscriber();
            sub.Subscribe(subChannel, (channel, message) =>
            {
                handler(channel, message);
            });
        }

        /// <summary>
        /// Redis发布订阅  发布
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="channel"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public long Publish(string channel, object msg)
        {
            var sub = _conn.GetSubscriber();
            return sub.Publish(channel, Serialize(msg));
        }

        /// <summary>
        /// Redis发布订阅  取消订阅
        /// </summary>
        /// <param name="channel"></param>
        public void Unsubscribe(string channel)
        {
            var sub = _conn.GetSubscriber();
            sub.Unsubscribe(channel);
        }

        /// <summary>
        /// Redis发布订阅  取消全部订阅
        /// </summary>
        public void UnsubscribeAll()
        {
            var sub = _conn.GetSubscriber();
            sub.UnsubscribeAll();
        }

        #endregion 发布订阅

        #region 辅助方法

        public T Do<T>(Func<IDatabase, T> func)
        {
            var database = _conn.GetDatabase(DbNum);
            return func(database);
        }

        public Task<T> DoAsync<T>(Func<IDatabase, Task<T>> func)
        {
            var database = _conn.GetDatabase(DbNum);
            return func(database);
        }

        public ITransaction CreateTransaction() => this.Database.CreateTransaction();

        public IServer GetServer(string hostAndPort)
        {
            return _conn.GetServer(hostAndPort);
        }

        public void Dispose()
        {
            //do nothing
        }

        #endregion 辅助方法

    }
}
