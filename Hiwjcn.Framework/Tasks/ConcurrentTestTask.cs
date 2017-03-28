using Lib.task;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.core;

namespace Hiwjcn.Framework.Tasks
{
    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution]
    public class ConcurrentTestTask : QuartzJobBase
    {
        public override bool AutoStart
        {
            get
            {
                return true;
            }
        }

        public override string Name
        {
            get
            {
                return "测试任务并发";
            }
        }

        public override ITrigger Trigger
        {
            get
            {
                return TriggerInterval(1);
            }
        }

        /// <summary>
        /// 不能使用async，不然不会等待结束
        /// </summary>
        /// <param name="context"></param>
        public override void Execute(IJobExecutionContext context)
        {
            AsyncHelper.RunSync(() => Task.Delay(3000));
            var i = 0;
        }
    }
}
