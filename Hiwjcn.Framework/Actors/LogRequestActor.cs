using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka;
using Akka.Actor;
using Lib.core;
using Lib.helper;
using Lib.extension;
using System.Threading;
using Hiwjcn.Core.Model.Sys;
using System.Diagnostics;
using Lib.ioc;
using Lib.data;

namespace Hiwjcn.Framework.Actors
{
    /// <summary>
    /// 异步记录请求记录
    /// </summary>
    public class LogRequestActor : ReceiveActor
    {
        public LogRequestActor()
        {
            this.Receive<ReqLogModel>(x =>
            {
                try
                {
                    x.Init("reqlog");
                    AppContext.Scope(s =>
                    {
                        return s.Resolve_<IRepository<ReqLogModel>>().Add(x);
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
