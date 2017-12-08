using Hiwjcn.Core.Domain.Sys;
using Lib.data;
using Lib.extension;
using Lib.ioc;
using Lib.task;
using Quartz;
using System;

namespace Hiwjcn.Framework.Tasks
{
    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution]
    public class ClearRequestLogTask : QuartzJobBase
    {
        public override string Name => "清理请求日志";

        public override bool AutoStart => true;

        public override ITrigger Trigger => this.TriggerIntervalInMinutes(5);

        public override void Execute(IJobExecutionContext context)
        {
            try
            {
                var expire = DateTime.Now.AddDays(-30);
                AppContext.Scope(s =>
                {
                    s.Resolve_<IRepository<ReqLogEntity>>().DeleteWhere(x => x.CreateTime < expire);
                    s.Resolve_<IRepository<CacheHitLogEntity>>().DeleteWhere(x => x.CreateTime < expire);
                    return true;
                });
            }
            catch (Exception e)
            {
                e.AddErrorLog(this.Name);
            }
        }
    }
}
