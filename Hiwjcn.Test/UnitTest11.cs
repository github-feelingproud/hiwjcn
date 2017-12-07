using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Reactive.Concurrency;
using System.Collections;
using Lib.helper;
using Lib.core;
using Lib.extension;
using Lib.io;
using System.Linq;
using Lib.task;
using Quartz;

namespace Hiwjcn.Test
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
                return this.TriggerIntervalInSeconds(1);
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
    [TestClass]
    public class UnitTest11
    {
        [TestMethod]
        public void TestMethod1()
        {
            Com.Range(100).ToObservable()
                .ObserveOn(Scheduler.ThreadPool)
                .ObserveOn(Scheduler.Default)
                .Subscribe(x =>
                {
                    Console.WriteLine(x);
                });
        }

        [TestMethod]
        public void fasdfasg()
        {
            using (var con = new TaskContainer(this.GetType().Assembly))
            {

            }
        }

    }
}
