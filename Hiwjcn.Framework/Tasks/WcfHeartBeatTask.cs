using Lib.extension;
using Lib.net;
using Lib.task;
using Lib.helper;
using Quartz;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Polly;

namespace Hiwjcn.Framework.Tasks
{
    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution]
    public class WcfHeartBeatTask : QuartzJobBase
    {
        public override string Name => "WCF服务心跳检测";

        public override bool AutoStart => true;

        public override ITrigger Trigger => this.TriggerInterval(15);

        public override void Execute(IJobExecutionContext context)
        {
            var queue = new Queue();
            //add task to queue

            void Worker()
            {
                while (true)
                {
                    try
                    {
                        var url = string.Empty;
                        lock (queue)
                        {
                            if (queue.Count <= 0) { break; }
                            url = (string)queue.Dequeue();
                        }
                        //request url and update last update time

                        Policy.Handle<Exception>().Retry(3).Execute(() => { });
                    }
                    catch (Exception e)
                    {
                        e.AddErrorLog();
                    }
                }
            }

            var thread_list = new List<Thread>(Com.Range(5).Select(x => new Thread(() => Worker()) { IsBackground = true }));
            //开启消费
            thread_list.ForEach(t => t.Start());
            //等待结束
            thread_list.ForEach(t => t.Join());
        }
    }
}
