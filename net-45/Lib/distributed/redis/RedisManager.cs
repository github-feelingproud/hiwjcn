using Lib.extension;
using System.Threading;

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
