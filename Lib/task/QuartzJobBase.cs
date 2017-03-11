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

        /// <summary>
        /// Cron表达式
        /// </summary>
        /// <param name="cron"></param>
        /// <returns></returns>
        protected ITrigger TriggerWithCron(string cron) =>
            BuildTrigger(t => t.WithSchedule(CronScheduleBuilder.CronSchedule(cron)).Build());
        
        /// <summary>
        /// 每天固定时间
        /// </summary>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <returns></returns>
        protected ITrigger TriggerDaily(int hour, int minute) =>
            BuildTrigger(t => t.WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(hour, minute)).Build());

        /// <summary>
        /// 间隔几秒
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        protected ITrigger TriggerInterval(int seconds) =>
            BuildTrigger(t => t.StartNow().WithSimpleSchedule(x => x.WithIntervalInSeconds(seconds).RepeatForever()).Build());

        /// <summary>
        /// 创建trigger
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        protected ITrigger BuildTrigger(Func<TriggerBuilder, ITrigger> func) => func(TriggerBuilder.Create());
    }
}
