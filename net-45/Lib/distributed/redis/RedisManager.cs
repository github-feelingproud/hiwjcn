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
using Polly.CircuitBreaker;
using System.Configuration;
using Polly;

namespace Lib.distributed.redis
{
    class MQTest
    {
        public MQTest()
        {
            var stop = false;
            using (var redis = new RedisHelper())
            {
                var db = redis.Database;
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
            }
        }
    }

}
