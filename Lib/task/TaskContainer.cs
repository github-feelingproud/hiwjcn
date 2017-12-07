using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Lib.extension;
using Quartz;
using Quartz.Impl.Matchers;
using Lib.helper;
using Quartz.Impl;

namespace Lib.task
{
    public class TaskContainer : IDisposable
    {
        [Obsolete]
        public IScheduler TaskScheduler { get => manager ?? throw new Exception("job容器没有生成"); }

        private readonly IScheduler manager;

        public TaskContainer(params Assembly[] ass) : this(ass.FindAllJobsAndCreateInstance()) { }

        public TaskContainer(IEnumerable<QuartzJobBase> jobs)
        {
            this.manager = StdSchedulerFactory.GetDefaultScheduler();
            this.manager.StartIfNotStarted();

            jobs = jobs?.Where(x => x != null && x.AutoStart).ToList();
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

            foreach (var job in jobs)
            {
                this.manager.AddJob(job);
            }
        }

        public List<ScheduleJobModel> GetAllTasks() => this.manager.GetAllTasks();

        public void AddJobManually(QuartzJobBase job)
        {
            this.manager.AddJob(job);
        }

        /// <summary>
        /// 关闭所有任务，请关闭task manager
        /// </summary>
        public void Dispose() => this.Dispose(false);

        public void Dispose(bool _WaitForJobsToComplete)
        {
            this.manager?.ShutdownIfStarted(_WaitForJobsToComplete);
        }

        public void PauseAll() => manager.PauseAll();

        public void ResumeAll() => manager.ResumeAll();

        public void PauseJob(string jobName, string groupName)
        {
            var key = JobKey.Create(jobName, groupName);
            manager.PauseJob(key);
        }

        public void ResumeJob(string jobName, string groupName)
        {
            var key = JobKey.Create(jobName, groupName);
            manager.ResumeJob(key);
        }

        public void DeleteJob(string jobName, string groupName)
        {
            var key = JobKey.Create(jobName, groupName);
            manager.DeleteJob(key);
        }

        public void TriggerJob(string jobName, string groupName)
        {
            var key = JobKey.Create(jobName, groupName);
            manager.TriggerJob(key);
        }
    }
}
