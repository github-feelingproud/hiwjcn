using Lib.extension;
using Lib.net;
using Lib.task;
using Quartz;
using System;
using System.Threading;
using Lib.core;
using Lib.ioc;
using Hiwjcn.Core.Infrastructure;

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

                //just for test
                return TriggerInterval(60);
            }
        }

        public override void Execute(IJobExecutionContext context)
        {
            try
            {
                Action<long, string> logger = (ms, name) =>
                {
                    $"{DateTime.Now}结束清理数据库结束，耗时：{ms}毫秒".AddBusinessInfoLog();
                };
                using (var timer = new CpuTimeLogger(logger))
                {
                    AppContext.Scope(s =>
                    {
                        var worker = s.Resolve_<IClearDataBaseService>();
                        worker.ClearCacheHitLog();
                        worker.ClearClient();
                        worker.ClearLoginLog();
                        worker.ClearPage();
                        worker.ClearRequestLog();
                        worker.ClearRole();
                        worker.ClearScope();
                        worker.ClearTag();
                        worker.ClearToken();
                        worker.ClearUser();
                        return true;
                    });
                }
            }
            catch (Exception e)
            {
                e.AddErrorLog("清理数据库发生异常");
            }
        }
    }
}
