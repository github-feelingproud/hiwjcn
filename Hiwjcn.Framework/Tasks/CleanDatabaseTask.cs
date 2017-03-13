using Lib.extension;
using Lib.net;
using Lib.task;
using Quartz;
using System;
using System.Threading;

namespace Hiwjcn.Framework.Tasks
{
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
                return TriggerDaily(2, 30);
            }
        }

        public override void Execute(IJobExecutionContext context)
        {
            var start = DateTime.Now;
            //do something
            Thread.Sleep(new Random((int)DateTime.Now.Ticks).Next(10) * 1000);
            var end = DateTime.Now;
            $"{start}开始清理数据库，{end}结束。耗时：{(end - start).TotalSeconds}秒".AddBusinessInfoLog();
        }
    }
}
