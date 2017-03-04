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
using Model.Task;

namespace Hiwjcn.Framework.Tasks
{
    /// <summary>
    /// 任务调度
    /// </summary>
    public static class TaskManager
    {
        public static string GetTriggerState(TriggerState state)
        {
            return state.ToString();
        }

        private static readonly object locker = new object();

        private static IScheduler manager = null;

        #region 获取任务的信息
        /// <summary>
        /// 获取task信息
        /// </summary>
        /// <returns></returns>
        public static List<ScheduleJobModel> GetAllTasks()
        {
            if (manager == null) { throw new Exception("请先开启服务"); }
            //所有任务
            var jobKeys = manager.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
            //正在运行的任务
            var runningJobs = manager.GetCurrentlyExecutingJobs();

            var list = new List<ScheduleJobModel>();
            foreach (var jobKey in jobKeys)
            {
                var triggers = manager.GetTriggersOfJob(jobKey);
                if (!ValidateHelper.IsPlumpList(triggers)) { continue; }
                foreach (var trigger in triggers)
                {
                    var job = new ScheduleJobModel();

                    job.JobName = jobKey.Name;
                    job.JobGroup = jobKey.Group;

                    job.TriggerName = trigger.Key.Name;
                    job.TriggerGroup = trigger.Key.Group;

                    //trigger information
                    job.StartTime = trigger.StartTimeUtc.DateTime;
                    job.PreTriggerTime = trigger.GetPreviousFireTimeUtc()?.DateTime;
                    job.NextTriggerTime = trigger.GetNextFireTimeUtc()?.DateTime;
                    job.JobStatus = GetTriggerState(manager.GetTriggerState(trigger.Key));

                    //判断是否在运行
                    job.IsRunning = runningJobs?.Any(x => x.JobDetail.Key == jobKey) ?? false;

                    list.Add(job);
                }
            }
            return list;
        }
        #endregion

        #region 开启和关闭任务
        public static ITrigger BuildCommonTrigger(int seconds) => TriggerBuilder.Create().StartNow().WithSimpleSchedule(x => x.WithIntervalInSeconds(seconds).RepeatForever()).Build();
        public static void InitTasks()
        {
            if (manager == null)
            {
                lock (locker)
                {
                    if (manager == null)
                    {
                        manager = StdSchedulerFactory.GetDefaultScheduler();
                    }
                }
            }
            manager.Clear();

            var job = JobBuilder.Create<WakeWebSiteTask>().WithIdentity("唤醒网站").Build();
            manager.ScheduleJob(job, BuildCommonTrigger(60));

            manager.Start();
        }

        /// <summary>
        /// 关闭所有任务，请关闭task manager
        /// </summary>
        public static void StopTasks() => manager?.Shutdown();
        #endregion

        #region 任务的手动干预
        public static void PauseJob(string jobName, string groupName)
        {
            var key = JobKey.Create(jobName, groupName);
            manager.PauseJob(key);
        }

        public static void ResumeJob(string jobName, string groupName)
        {
            var key = JobKey.Create(jobName, groupName);
            manager.ResumeJob(key);
        }

        public static void DeleteJob(string jobName, string groupName)
        {
            var key = JobKey.Create(jobName, groupName);
            manager.DeleteJob(key);
        }

        public static void TriggerJob(string jobName, string groupName)
        {
            var key = JobKey.Create(jobName, groupName);
            manager.TriggerJob(key);
        }
        #endregion
    }
}
