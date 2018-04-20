using Lib.extension;
using Lib.net;
using Lib.task;
using Quartz;
using System;
using System.Threading;
using Lib.core;
using Lib.ioc;
using Lib.data;
using System.Linq;
using Hiwjcn.Core.Domain.Auth;
using Lib.infrastructure.entity;
using Lib.data.ef;
using Lib.infrastructure.entity.auth;
using Hiwjcn.Core.Data;
using System.Threading.Tasks;

namespace Hiwjcn.Framework.Tasks
{
    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution]
    public class ClearExpiredTokenTask : QuartzJobBase
    {
        public override string Name => "清理过期的Token";

        public override bool AutoStart => true;

        public override ITrigger Trigger => this.TriggerIntervalInMinutes(5);

        public override async Task Execute(IJobExecutionContext context)
        {
            try
            {
                using (var s = AutofacIocContext.Instance.Scope())
                {
                    var repo = s.Resolve_<IMSRepository<AuthToken>>();
                    repo.PrepareSession(db =>
                    {
                        var now = DateTime.Now;
                        var token_set = db.Set<AuthToken>();

                        //delete tokens
                        token_set.RemoveRange(token_set.Where(x => x.ExpiryTime < now));
                        db.SaveChanges();
                    });
                }
                "成功清理过期token，以及相关信息".AddBusinessInfoLog();
            }
            catch (Exception e)
            {
                e.AddErrorLog(this.Name);
            }
        }
    }
}
