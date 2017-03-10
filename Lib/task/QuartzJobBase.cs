using System;
using System.Collections.Generic;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Quartz;
using Quartz.Job;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using Quartz.Collection;
using Lib.helper;
using Lib.core;

namespace Lib.task
{
    public abstract class QuartzJobBase : IJob
    {
        public abstract string Name { get; }

        public abstract bool Start { get; }

        public abstract ITrigger Trigger { get; }

        public abstract void Execute(IJobExecutionContext context);

        protected ITrigger BuildCommonTrigger(int seconds) => 
            BuildTrigger(t => t.StartNow().WithSimpleSchedule(x => x.WithIntervalInSeconds(seconds).RepeatForever()).Build());
        protected ITrigger BuildTrigger(Func<TriggerBuilder, ITrigger> func) => func(TriggerBuilder.Create());
    }
}
