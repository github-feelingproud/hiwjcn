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
using System.Reflection;

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
                    job.StartTime = trigger.StartTimeUtc.LocalDateTime;
                    job.PreTriggerTime = trigger.GetPreviousFireTimeUtc()?.LocalDateTime;
                    job.NextTriggerTime = trigger.GetNextFireTimeUtc()?.LocalDateTime;
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
        public static void StartAllTasks(Assembly[] ass)
        {
            if (ass.Select(x => x.FullName).Distinct().Count() != ass.Count())
            {
                throw new Exception("无法启动任务：传入重复的程序集");
            }
            var jobs = new List<QuartzJobBase>();
            foreach (var a in ass)
            {
                jobs.AddRange(a.FindJobTypes().Select(x => (QuartzJobBase)Activator.CreateInstance(x)));
            }

            StartAllTasks(jobs);
        }

        public static void StartAllTasks(List<QuartzJobBase> jobs)
        {
            jobs = jobs.Where(x => x.AutoStart).ToList();
            if (!ValidateHelper.IsPlumpList(jobs))
            {
                "没有需要启动的任务".AddBusinessInfoLog();
                return;
            }
            //任务检查
            if (jobs.Select(x => x.Name).Distinct().Count() != jobs.Count())
            {
                throw new Exception("注册的任务中存在重名");
            }
            if (jobs.Any(x => x.Trigger == null))
            {
                throw new Exception("注册的任务中有些Trigger没有定义");
            }

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

            foreach (var job in jobs)
            {
                var job_to_run = JobBuilder.Create(job.GetType()).WithIdentity(job.Name).Build();
                manager.ScheduleJob(job_to_run, job.Trigger);
            }

            manager.Start();
        }

        [Obsolete("请调用StartAllTasks")]
        public static void InitTasks(Assembly[] ass) => StartAllTasks(ass);

        /// <summary>
        /// 关闭时是否等待任务完成
        /// </summary>
        public static bool? _WaitForJobsToComplete = null;

        /// <summary>
        /// 关闭所有任务，请关闭task manager
        /// </summary>
        public static void Dispose() => manager?.ShutdownIfStarted(_WaitForJobsToComplete ?? false);
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

    /// <summary>
    /// quartz扩展
    /// </summary>
    public static class QuartzExtension
    {
        /// <summary>
        /// 如果任务开启就关闭
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="waitForJobsToComplete"></param>
        public static void ShutdownIfStarted(this IScheduler manager, bool waitForJobsToComplete = false)
        {
            if (!waitForJobsToComplete)
            {
                $"任务关闭不会等待任务完成，肯能导致数据不完整，你可以设置{nameof(waitForJobsToComplete)}来调整".AddBusinessWarnLog();
            }
            if (manager.IsStarted)
            {
                manager.Shutdown(waitForJobsToComplete);
            }
        }

        /// <summary>
        /// 找到任务
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Type[] FindJobTypes(this Assembly a)
        {
            return a.GetTypes().Where(x => x.IsNormalClass() && x.IsAssignableTo_<QuartzJobBase>()).ToArray();
        }
    }

}
