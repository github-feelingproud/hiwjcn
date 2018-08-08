using System;
using System.Collections.Generic;
using System.Text;

namespace Lib.redis
{
    public interface IRedisAll :
        IRedis, IRedisHash, IRedisSet,
        IRedisSortedSet, IRedisKey, IRedisString,
        IRedisPubSub, IRedisList
    {
        //
    }
}
