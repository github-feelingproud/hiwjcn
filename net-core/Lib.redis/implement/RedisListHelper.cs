using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Lib.distributed.redis
{
    /// <summary>
    /// list
    /// </summary>
    public partial class RedisHelper
    {
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
    }
}
