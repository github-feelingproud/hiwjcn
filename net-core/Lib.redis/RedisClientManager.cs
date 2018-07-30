﻿using Lib.core;
using Lib.extension;
using StackExchange.Redis;

namespace Lib.distributed.redis
{
    /// <summary>
    /// redis 链接管理
    /// </summary>
    public class RedisClientManager : StaticClientManager<ConnectionMultiplexer>
    {
        public static readonly RedisClientManager Instance = new RedisClientManager();

        public override string DefaultKey
        {
            get
            {
                return ConfigHelper.Instance.RedisConnectionString;
            }
        }

        public override bool CheckClient(ConnectionMultiplexer ins)
        {
            return ins != null && ins.IsConnected;
        }

        public override ConnectionMultiplexer CreateNewClient(string key)
        {
            var pool = ConnectionMultiplexer.Connect(key);
            pool.ConnectionFailed += (sender, e) => { e.Exception?.AddErrorLog("Redis连接失败:" + e.FailureType); };
            pool.ConnectionRestored += (sender, e) => { "Redis连接恢复".AddBusinessInfoLog(); };
            pool.ErrorMessage += (sender, e) => { e.Message?.AddErrorLog("Redis-ErrorMessage"); };
            pool.InternalError += (sender, e) => { e.Exception?.AddErrorLog("Redis内部错误"); };
            return pool;
        }

        public override void DisposeClient(ConnectionMultiplexer ins)
        {
            ins?.Dispose();
        }
    }
}
