using Lib.task;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.core;
using System.Threading;
using Lib.extension;
using Lib.helper;

namespace Hiwjcn.Framework.Tasks
{
    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution]
    public class LongTimeTask : QuartzJobBase
    {
        public override string Name => "模拟耗时任务";

        public override bool AutoStart => true;

        public override ITrigger Trigger => this.TriggerIntervalInSeconds(60);

        public override async Task Execute(IJobExecutionContext context)
        {
            try
            {
                Thread.Sleep(1000 * 3 * 10);
            }
            catch (Exception e)
            {
                e.AddErrorLog(this.Name);
            }
        }
    }
}
