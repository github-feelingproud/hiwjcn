using System;

namespace Lib.distributed.redis
{
    /// <summary>
    /// 目前只是思路
    /// </summary>
    public class RedisFireWall
    {
        private readonly RedisHelper redis;
        private readonly TimeSpan expire;
        private readonly double limit;

        public RedisFireWall() : this(null, 1, TimeSpan.FromMinutes(1), 30)
        { }

        public RedisFireWall(string connection_string, int db, TimeSpan expire, double limit)
        {
            this.redis = new RedisHelper(db, connection_string);
            this.expire = expire;
            this.limit = limit;
            if (this.limit <= 0) { throw new Exception("limit不能小于1"); }
        }

        public bool Hit(string key)
        {
            //var first = !this.redis.KeyExists(key);
            var count = this.redis.StringIncrement(key, 1);
            var first = count == 1;
            if (first)
            {
                if (!this.redis.KeyExpire(key, TimeSpan.FromMinutes(1))) { throw new Exception("无法设置key过期"); }
            }

            return count <= this.limit;
        }

        public double HitCount(string key)
        {
            var count = this.redis.StringGet<double>(key);

            return count;
        }

    }
}
