using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lib.distributed.redis
{
    /// <summary>
    /// set
    /// </summary>
    public partial class RedisHelper
    {
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
    }
}
