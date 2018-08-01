using System;
using System.Collections.Generic;
using System.Text;

namespace Lib.redis
{
    public class RedisCacheProviderConfig
    {
        public int DbNumber { get; private set; }

        public RedisCacheProviderConfig(int db) => this.DbNumber = db;
    }
}
