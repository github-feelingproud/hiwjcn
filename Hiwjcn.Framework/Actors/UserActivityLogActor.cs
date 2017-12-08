using Akka.Actor;
using Hiwjcn.Core.Domain.Sys;
using Lib.core;
using Lib.data;
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
                    AppContext.Scope(s =>
                    {
                        s.Resolve_<IRepository<UserActivityEntity>>().Add(x);
                        return true;
                    });
                }
                catch (Exception e)
                {
                    e.AddErrorLog();
                }
            });
        }
    }
}
