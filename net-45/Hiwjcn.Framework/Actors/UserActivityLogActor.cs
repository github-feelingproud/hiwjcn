using Akka.Actor;
using Hiwjcn.Core.Data;
using Hiwjcn.Core.Domain.Sys;
using Lib.core;
using Lib.data;
using Lib.data.ef;
using Lib.extension;
using Lib.ioc;
using System;

namespace Hiwjcn.Framework.Actors
{
    public class UserActivityLogActor : ReceiveActor
    {
        public UserActivityLogActor()
        {
            this.Receive<UserActivityEntity>(x =>
            {
                try
                {
                    x.Init("ua");
                    if (!x.IsValid(out var msg))
                    {
                        msg.AddBusinessInfoLog();
                        return;
                    }
                    using (var s = AutofacIocContext.Instance.Scope())
                    {
                        s.Resolve_<IMSRepository<UserActivityEntity>>().Add(x);
                    }
                }
                catch (Exception e)
                {
                    e.AddErrorLog();
                }
            });
        }
    }
}
