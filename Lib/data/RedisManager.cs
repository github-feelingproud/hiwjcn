using Lib.core;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Lib.extension;

namespace Lib.data
{
    /*
     个人觉得对NoSql数据库的操作不要使用异步，因为本身响应就很快，不会阻塞
     
     个人觉得对NoSql数据库的操作不要使用异步，因为本身响应就很快，不会阻塞
     
     个人觉得对NoSql数据库的操作不要使用异步，因为本身响应就很快，不会阻塞
         */

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
            handler.Invoke(RedisConnectionManager.StoreInstance());
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
        private static readonly StoreInstanceDict<ConnectionMultiplexer> ConnectionCache = new StoreInstanceDict<ConnectionMultiplexer>();

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
                            _instance = StoreInstance();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// 存储实例
        /// </summary>
        /// <param name="conStr"></param>
        /// <returns></returns>
        public static ConnectionMultiplexer StoreInstance(string conStr = null)
        {
            conStr = conStr ?? DefaultConnectionString;
            var ins = ConnectionCache.StoreInstance(conStr
                , () =>
                {
                    return ConnectionMultiplexer.Connect(conStr);
                }
                , x =>
                {
                    return x != null && x.IsConnected;
                }
                , x => x?.Dispose());
            return ins;
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
    /// redis 链接管理
    /// </summary>
    public class RedisClientManager : StaticClientManager<ConnectionMultiplexer>
    {
        public static readonly RedisClientManager Instance = new RedisClientManager();

        public override bool CheckInstance(ConnectionMultiplexer ins)
        {
            return ins != null && ins.IsConnected;
        }

        public override ConnectionMultiplexer CreateInstance(string key)
        {
            return ConnectionMultiplexer.Connect(key);
        }

        public override void Dispose()
        {
            foreach (var kv in db)
            {
                kv.Value?.Dispose();
            }
        }

        public override void DisposeBrokenInstance(ConnectionMultiplexer ins)
        {
            ins?.Dispose();
        }
    }

    /// <summary>
    /// Redis操作
    /// https://github.com/qq1206676756/RedisHelp
    /// </summary>
    public class RedisHelper : SerializeBase, IDisposable
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
                RedisConnectionManager.StoreInstance(readWriteHosts);
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

    class MQTest
    {
        public MQTest()
        {
            var stop = false;
            RedisManager.PrepareDataBase(db =>
            {
                while (true)
                {
                    if (stop) { break; }

                    var data = db.ListRightPop("es-index");
                    if (!data.HasValue)
                    {
                        Thread.Sleep(50);
                        continue;
                    }
                    //handler data
                    string json = data;
                    var model = json.JsonToEntity<RedisHelper>();
                }
                return true;
            });
        }
    }
}
