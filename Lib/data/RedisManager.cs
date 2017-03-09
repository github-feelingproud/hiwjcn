using Lib.core;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Lib.data
{
    /// <summary>
    /// redis基类，实现序列化和反序列化
    /// </summary>
    public abstract class RedisBase
    {
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected virtual byte[] Serialize(object item)
        {
            if (item != null)
            {
                var jsonString = JsonConvert.SerializeObject(item);
                return Encoding.UTF8.GetBytes(jsonString);
            }
            return new byte[] { };
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializedObject"></param>
        /// <returns></returns>
        protected virtual T Deserialize<T>(byte[] serializedObject)
        {
            if (serializedObject?.Length > 0)
            {
                var jsonString = Encoding.UTF8.GetString(serializedObject);
                return JsonConvert.DeserializeObject<T>(jsonString);
            }
            return default(T);
        }
    }

    /// <summary>
    /// redis 老帮助类
    /// </summary>
    public static class RedisManager
    {
        /// <summary>
        /// 获取redis连接
        /// </summary>
        /// <param name="handler"></param>
        public static void PrepareConnection(Func<ConnectionMultiplexer, bool> handler)
        {
            handler.Invoke(RedisConnectionManager.GetConnectionAndCache());
        }

        /// <summary>
        /// 获取redis database
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="db_no"></param>
        public static void PrepareDataBase(Func<IDatabase, bool> callback, int? db_no = null)
        {
            PrepareConnection(con =>
            {
                IDatabase db = null;

                if (db_no != null && db_no.HasValue)
                {
                    db = con.GetDatabase(db_no.Value);
                }
                else
                {
                    db = con.GetDatabase();
                }
                callback.Invoke(db);

                return true;
            });
        }

        /// <summary>
        /// 防止多次尝试密码逻辑，还没开始使用
        /// </summary>
        /// <param name="key"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static string RedisPreventTryLogin(string key, Func<bool> func)
        {
            var time = DateTime.Now.AddMinutes(-30);

            var client = new RedisHelper();
            var list = client.StringGet<List<DateTime>>(key);
            list = Lib.helper.ConvertHelper.NotNullList(list).Where(x => x > time).ToList();

            if (list.Count >= 3)
            {
                //先判断验证码，否则提示错误
                return "超过尝试次数";
            }

            if (!func.Invoke())
            {
                //登录失败，添加错误时间
                list.Add(DateTime.Now);
            }
            client.StringSet(key, list);
            return "";
        }
    }

    /// <summary>
    /// ConnectionMultiplexer对象管理帮助类
    /// https://github.com/qq1206676756/RedisHelp
    /// </summary>
    public static class RedisConnectionManager
    {
        //"127.0.0.1:6379,allowadmin=true
        private static readonly string DefaultConnectionString = ConfigHelper.Instance.RedisConnectionString;
        private static readonly object get_instance_locker = new object();
        private static readonly object add_cache_locker = new object();
        private static readonly ConcurrentDictionary<string, ConnectionMultiplexer> ConnectionCache = new ConcurrentDictionary<string, ConnectionMultiplexer>();

        private static ConnectionMultiplexer _instance { get; set; }
        /// <summary>
        /// 单例获取
        /// </summary>
        public static ConnectionMultiplexer Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (get_instance_locker)
                    {
                        if (_instance == null || !_instance.IsConnected)
                        {
                            _instance = GetConnectionAndCache();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// 缓存获取
        /// </summary>
        /// <param name="conStr"></param>
        /// <returns></returns>
        public static ConnectionMultiplexer GetConnectionAndCache(string conStr = null)
        {
            conStr = conStr ?? DefaultConnectionString;
            if (ConnectionCache.ContainsKey(conStr))
            {
                //有缓存
                var con = ConnectionCache[conStr];
                if (con != null && con.IsConnected)
                {
                    return con;
                }
                else
                {
                    ConnectionCache.Keys.Remove(conStr);
                }
            }
            //没有缓存
            var connection = ConnectionMultiplexer.Connect(conStr);
            //把连接对象添加到缓存，防止多线程重复添加
            if (!ConnectionCache.ContainsKey(conStr))
            {
                lock (add_cache_locker)
                {
                    if (!ConnectionCache.ContainsKey(conStr))
                    {
                        ConnectionCache[conStr] = connection;
                    }
                }
            }
            return ConnectionCache[conStr];
        }

        public static void Dispose()
        {
            foreach (var kv in ConnectionCache)
            {
                kv.Value?.Dispose();
            }
        }
    }

    /// <summary>
    /// Redis操作
    /// https://github.com/qq1206676756/RedisHelp
    /// </summary>
    public class RedisHelper : RedisBase, IDisposable
    {
        private int DbNum { get; }

        private readonly ConnectionMultiplexer _conn;

        public ConnectionMultiplexer Connection { get { return _conn; } }

        public RedisHelper(int dbNum = 0) : this(dbNum, null) { }

        public RedisHelper(int dbNum, string readWriteHosts)
        {
            DbNum = dbNum;
            _conn =
                string.IsNullOrWhiteSpace(readWriteHosts) ?
                RedisConnectionManager.Instance :
                RedisConnectionManager.GetConnectionAndCache(readWriteHosts);
        }

        #region String

        /// <summary>
        /// 保存一个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
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
        /// 入栈
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void ListLeftPush(string key, object value)
        {
            Do(db => db.ListLeftPush(key, Serialize(value)));
        }

        /// <summary>
        /// 出栈
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T ListLeftPop<T>(string key)
        {
            return Do(db =>
            {
                var value = db.ListLeftPop(key);
                return Deserialize<T>(value);
            });
        }

        /// <summary>
        /// 获取集合中的数量
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public long ListLength(string key)
        {
            return Do(redis => redis.ListLength(key));
        }

        #endregion List

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

        private T Do<T>(Func<IDatabase, T> func)
        {
            var database = _conn.GetDatabase(DbNum);
            return func(database);
        }

        public ITransaction CreateTransaction()
        {
            return GetDatabase().CreateTransaction();
        }

        public IDatabase GetDatabase()
        {
            return _conn.GetDatabase(DbNum);
        }

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

    /// <summary>
    /// Redis分布式锁
    /// https://github.com/KidFashion/redlock-cs
    /// </summary>
    public class RedisDistributedLock : IDisposable
    {
        public int DB_NUM { get; set; } = 1;
        private string _key { get; set; }
        private byte[] _value { get; set; }
        /// <summary>
        /// 分布式锁
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="retry_count">尝试次数</param>
        /// <param name="retry_delay_ms">尝试延迟，毫秒</param>
        /// <param name="expiry_seconds">锁过期时间，秒</param>
        public RedisDistributedLock(string key, int timeout_ms = 1000 * 10, int expiry_seconds = 30)
        {
            this._key = key;
            this._value = Guid.NewGuid().ToByteArray();
            Retry(timeout_ms, () =>
            {
                var success = false;
                RedisManager.PrepareDataBase(db =>
                {
                    success = db.StringSet(this._key, this._value, expiry: TimeSpan.FromSeconds(expiry_seconds), when: When.NotExists);
                    return true;
                }, DB_NUM);
                return success;
            });
        }
        private void Retry(int timeout_ms, Func<bool> createLock)
        {
            var start = DateTime.Now;
            while (true)
            {
                if (createLock())
                {
                    break;
                }
                if ((DateTime.Now - start).TotalMilliseconds > timeout_ms)
                {
                    throw new Exception($"尝试获取分布式锁失败，超时时间：{timeout_ms}ms");
                }
                Thread.Sleep(200);
            }
        }
        private const string UnlockScript = @"
            if redis.call(""get"",KEYS[1]) == ARGV[1] then
                return redis.call(""del"",KEYS[1])
            else
                return 0
            end";
        public void Dispose()
        {
            RedisManager.PrepareDataBase(db =>
            {
                RedisKey[] key = { this._key };
                RedisValue[] values = { this._value };
                var res = db.ScriptEvaluate(UnlockScript, key, values);
                return true;
            }, DB_NUM);
        }
    }
}
