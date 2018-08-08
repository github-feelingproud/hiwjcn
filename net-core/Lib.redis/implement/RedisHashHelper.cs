using Lib.data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lib.distributed.redis
{
    /// <summary>
    /// hash
    /// </summary>
    public partial class RedisHelper
    {
        public bool HashSet(string key, string k, object value) =>
            Do(db => db.HashSet(key, k, Serialize(value)));

        public bool HashDelete(string key, string k) =>
            Do(db => db.HashDelete(key, k));
    }
}
