using System;
using System.Collections.Generic;
using System.Text;

namespace Lib.redis
{
    public class RedisDistributedLockConfig
    {
        public int DbNumber { get; private set; }

        public RedisDistributedLockConfig(int db)
        {
            this.DbNumber = db;
        }
    }
}
