using Lib.core;
using Lib.extension;
using Lib.helper;
using Lib.ioc;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lib.task
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
        /// <summary>
        /// 初始化任务，只能调用一次
        /// </summary>
        public static void InitTasks()
        {
            if (manager != null)
            {
                throw new Exception("调度对象已经初始化");
            }
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

            //从IOC中找到任务，并执行
            QuartzJobBase[] jobs = null;
            try
            {
                jobs = AppContext.GetAllObject<QuartzJobBase>();
            }
            catch (Exception e)
            {
                e.AddErrorLog();
            }
            jobs = ConvertHelper.NotNullList(jobs).Where(x => x.Start).ToArray();
            if (ValidateHelper.IsPlumpList(jobs))
            {
                if (jobs.Select(x => x.Name).Distinct().Count() != jobs.Count())
                {
                    throw new Exception("注册的任务中存在重名");
                }
                if (jobs.Any(x => x.Trigger == null))
                {
                    throw new Exception("注册的任务中有些Trigger没有定义");
                }
                foreach (var job in jobs)
                {
                    var job_to_run = JobBuilder.Create(job.GetType()).WithIdentity(job.Name).Build();
                    manager.ScheduleJob(job_to_run, job.Trigger);
                }
            }
            else
            {
                return;
            }

            manager.Start();
        }

        /// <summary>
        /// 关闭所有任务，请关闭task manager
        /// </summary>
        public static void Dispose() => manager?.Shutdown();
        #endregion

        #region 任务的手动干预
        /// <summary>
        /// 全部暂停
        /// </summary>
        public static void PauseAll() => manager.PauseAll();

        /// <summary>
        /// 全部继续
        /// </summary>
        public static void ResumeAll() => manager.ResumeAll();

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
