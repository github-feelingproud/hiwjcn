using Akka.Actor;
using Hiwjcn.Core.Data;
using Hiwjcn.Core.Domain.Sys;
using Lib.data;
using Lib.data.ef;
using Lib.extension;
using Lib.ioc;
using System;
using System.Threading;

namespace Hiwjcn.Framework.Actors
{
    /// <summary>
    /// 异步记录请求记录
    /// </summary>
    public class LogRequestActor : ReceiveActor
    {
        public LogRequestActor()
        {
            this.Receive<ReqLogEntity>(x =>
            {
                try
                {
                    x.Init("reqlog");
                    using (var s = AutofacIocContext.Instance.Scope())
                    {
                        s.Resolve_<IMSRepository<ReqLogEntity>>().Add(x);
                    }
                }
                catch (Exception e)
                {
                    e.DebugInfo();
                }
            });
        }
    }

    public class TestActor : ReceiveActor
    {
        public TestActor()
        {
            this.Receive<string>(x =>
            {
                Thread.Sleep(500);
                x?.AddBusinessInfoLog();

                //这里stop似乎就可以释放资源
                //测试是这样的，还有待求证
                Context.Stop(this.Self);
            });
        }
    }
}
