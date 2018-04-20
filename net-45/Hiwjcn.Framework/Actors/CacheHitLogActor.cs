using Akka.Actor;
using Hiwjcn.Core.Data;
using Hiwjcn.Core.Domain.Sys;
using Lib.data;
using Lib.data.ef;
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

                    using (var s = AutofacIocContext.Instance.Scope())
                    {
                        s.Resolve_<IMSRepository<CacheHitLogEntity>>().Add(x);
                    }
                }
                catch (Exception e)
                {
                    e.DebugInfo();
                }
            });
        }
    }
}
