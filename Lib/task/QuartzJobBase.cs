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
        /// <summary>
        /// 任务名，不能重复
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// 是否自动启动
        /// </summary>
        public abstract bool AutoStart { get; }

        /// <summary>
        /// 调度规则
        /// </summary>
        public abstract ITrigger Trigger { get; }

        /// <summary>
        /// 任务的具体实现
        /// </summary>
        /// <param name="context"></param>
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
        /// 每月的某一时间
        /// </summary>
        /// <param name="dayOfMonth"></param>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <returns></returns>
        protected ITrigger TriggerMonthly(int dayOfMonth, int hour, int minute) =>
            BuildTrigger(t => t.WithSchedule(CronScheduleBuilder.MonthlyOnDayAndHourAndMinute(dayOfMonth, hour, minute)).Build());

        /// <summary>
        /// 每周某一天执行
        /// </summary>
        /// <param name="dayOfWeek"></param>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <returns></returns>
        protected ITrigger TriggerWeekly(DayOfWeek dayOfWeek, int hour, int minute) =>
            BuildTrigger(t => t.WithSchedule(CronScheduleBuilder.WeeklyOnDayAndHourAndMinute(dayOfWeek, hour, minute)).Build());

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

        private void Test()
        {
            var start = DateTime.Now.AddHours(1);
            start = new DateTime(start.Year, start.Month, start.Day, start.Hour, 0, 0);

            BuildTrigger(t => t.StartAt(DateTimeOffset.Now.Add(start - DateTime.Now)).WithSimpleSchedule(x => x.WithIntervalInHours(1).RepeatForever()).Build());
        }
    }

    public abstract class QuartzJobBase_ : QuartzJobBase
    {
        public override void Execute(IJobExecutionContext context)
        {
            this.ExecuteJob(context);
        }

        public abstract void ExecuteJob(IJobExecutionContext context);
    }
}