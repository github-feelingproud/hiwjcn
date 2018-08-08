using System;
using System.Collections.Generic;
using System.Text;

namespace Lib.distributed.redis
{
    /// <summary>
    /// sorted set
    /// </summary>
    public partial class RedisHelper
    {
        #region SortedSet
        public bool SortedSetAdd(string key, object value, double score) => Do(db => db.SortedSetAdd(key, Serialize(value), score));
        public bool SortedSetRemove<T>(string key, object value) => Do(db => db.SortedSetRemove(key, Serialize(value)));
        #endregion
    }
}
