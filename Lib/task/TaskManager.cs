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
    [Obsolete("用" + nameof(TaskContainer))]
    public static class TaskManager
    {
        //private static readonly Lazy_<TaskContainer> _manager = new Lazy_<TaskContainer>(() =>
        //{
        //    var con = new TaskContainer();
        //    con.AddJobFromAssembly();
        //    con.Start();
        //    return con;
        //});


        private static readonly object locker = new object();

        private static IScheduler manager = null;

        public static IScheduler TaskScheduler { get => manager ?? throw new Exception("job容器没有生成"); }

        #region 获取任务的信息
        /// <summary>
        /// 获取task信息
        /// </summary>
        /// <returns></returns>
        public static List<ScheduleJobModel> GetAllTasks() => manager.GetAllTasks_();
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
                jobs.AddRange(a.FindJobTypes_().Select(x => (QuartzJobBase)Activator.CreateInstance(x)));
            }

            StartAllTasks(jobs);
        }

        private static readonly List<QuartzJobBase> _jobs = new List<QuartzJobBase>();

        public static void AddJobManually(QuartzJobBase job)
        {
            if (manager != null) { throw new Exception("job容器已经启动，不支持此方法添加新job"); }

            _jobs.Add(job);
        }

        public static void ClearAllJobsAddedManually() => _jobs.Clear();

        public static void StartAllTasks(List<QuartzJobBase> jobs)
        {
            if (_jobs.Count > 0)
            {
                jobs.AddRange(_jobs);
            }

            jobs = jobs.Where(x => x != null && x.AutoStart).ToList();
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
            if (jobs.Any(x => x.CachedTrigger == null))
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
                manager.ScheduleJob(job_to_run, job.CachedTrigger);
            }

            manager.Start();
        }

        /// <summary>
        /// 关闭时是否等待任务完成
        /// </summary>
        public static bool? _WaitForJobsToComplete = null;

        /// <summary>
        /// 关闭所有任务，请关闭task manager
        /// </summary>
        public static void Dispose() => manager?.ShutdownIfStarted_(_WaitForJobsToComplete ?? false);
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
