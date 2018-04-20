using Hiwjcn.Core.Data;
using Hiwjcn.Core.Domain.Sys;
using Lib.data;
using Lib.data.ef;
using Lib.extension;
using Lib.ioc;
using Lib.task;
using Quartz;
using System;
using System.Threading.Tasks;

namespace Hiwjcn.Framework.Tasks
{
    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution]
    public class ClearRequestLogTask : QuartzJobBase
    {
        public override string Name => "清理请求日志";

        public override bool AutoStart => true;

        public override ITrigger Trigger => this.TriggerIntervalInMinutes(5);

        public override async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var expire = DateTime.Now.AddDays(-30);
                using (var s = AutofacIocContext.Instance.Scope())
                {
                    s.Resolve_<IMSRepository<ReqLogEntity>>().DeleteWhere(x => x.CreateTime < expire);
                    s.Resolve_<IMSRepository<CacheHitLogEntity>>().DeleteWhere(x => x.CreateTime < expire);
                }
            }
            catch (Exception e)
            {
                e.AddErrorLog(this.Name);
            }
        }
    }
}
