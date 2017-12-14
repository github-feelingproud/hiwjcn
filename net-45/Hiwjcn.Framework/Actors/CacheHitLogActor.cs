using Akka.Actor;
using Hiwjcn.Core.Domain.Sys;
using Lib.data;
using Lib.extension;
using Lib.ioc;
using System;

namespace Hiwjcn.Framework.Actors
{
    /// <summary>
    /// 记录缓存命中的情况
    /// </summary>
    public class CacheHitLogActor : ReceiveActor
    {
        public CacheHitLogActor()
        {
            this.Receive<CacheHitLogEntity>(x =>
            {
                try
                {
                    x.Init("cachehitlog");

                    $"缓存命中情况：{x.ToJson()}".AddBusinessInfoLog();

                    AppContext.Scope(s =>
                    {
                        s.Resolve_<IRepository<CacheHitLogEntity>>().Add(x);
                        return true;
                    });
                }
                catch (Exception e)
                {
                    e.DebugInfo();
                }
            });
        }
    }
}
