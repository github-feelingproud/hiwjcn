using StackExchange.Redis;

namespace Lib.distributed.redis
{
    public static class RedisExtension
    {
        public static IDatabase SelectDatabase(this IConnectionMultiplexer con, int? db)
        {
            if (db == null)
            {
                return con.GetDatabase();
            }
            else
            {
                return con.GetDatabase(db.Value);
            }
        }
    }
}
