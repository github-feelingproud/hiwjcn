using Hiwjcn.Service.MemberShip;
using Lib.core;
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
    public class CleanDatabaseTask : QuartzJobBase
    {
        public override string Name
        {
            get
            {
                return "清理数据库";
            }
        }

        public override bool AutoStart
        {
            get
            {
                return true;
            }
        }

        public override ITrigger Trigger
        {
            get
            {
                //早上2.30清理数据库
                //return TriggerDaily(2, 30);

                //2小时执行一次
                return this.TriggerIntervalInHours(2);
            }
        }

        public override async Task Execute(IJobExecutionContext context)
        {
            try
            {
                Action<long, string> logger = (ms, name) =>
                {
                    $"{DateTime.Now}结束清理数据库结束，耗时：{ms}毫秒".AddBusinessInfoLog();
                };
                using (var timer = new CpuTimeLogger(logger))
                {
                    using (var s = AutofacIocContext.Instance.Scope())
                    {
                        var worker = s.Resolve_<IClearDataBaseService>();
                        /* 这里不要清理，这里关联的用户是usermodel，开启会删除重要数据！！！
                        worker.ClearCacheHitLog();
                        worker.ClearClient();
                        worker.ClearScope();
                        worker.ClearToken();

                        worker.ClearLoginLog();
                        worker.ClearPage();
                        worker.ClearRequestLog();
                        worker.ClearRole();
                        worker.ClearPermission();
                        worker.ClearTag();
                        worker.ClearUser();*/
                    }
                }
            }
            catch (Exception e)
            {
                e.AddErrorLog($"{this.Name}:清理数据库发生异常");
            }
        }
    }
}
