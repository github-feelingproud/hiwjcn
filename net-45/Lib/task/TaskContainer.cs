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
        private readonly IScheduler _manager;

        public TaskContainer()
        {
            this._manager = StdSchedulerFactory.GetDefaultScheduler();
        }

        public IScheduler TaskScheduler => this._manager ?? throw new Exception("job容器没有生成");

        public void AddJobFromAssembly(params Assembly[] ass)
        {
            var jobs = ass.FindAllJobsAndCreateInstance_();
            this.AddJob(jobs.ToArray());
        }

        public void AddJob(params QuartzJobBase[] jobs)
        {
            if (!ValidateHelper.IsPlumpList(jobs)) { return; }
            foreach (var job in jobs)
            {
                this._manager.AddJob_(job);
            }
        }

        /// <summary>
        /// 开启
        /// </summary>
        public void Start() => this._manager.StartIfNotStarted_();

        public List<ScheduleJobModel> GetAllTasks() => this._manager.GetAllTasks_();

        public void PauseAll() => this._manager.PauseAll();

        public void ResumeAll() => this._manager.ResumeAll();

        public void PauseJob(string jobName, string groupName)
        {
            var key = JobKey.Create(jobName, groupName);
            this._manager.PauseJob(key);
        }

        public void ResumeJob(string jobName, string groupName)
        {
            var key = JobKey.Create(jobName, groupName);
            this._manager.ResumeJob(key);
        }

        public void DeleteJob(string jobName, string groupName)
        {
            var key = JobKey.Create(jobName, groupName);
            this._manager.DeleteJob(key);
        }

        public void TriggerJob(string jobName, string groupName)
        {
            var key = JobKey.Create(jobName, groupName);
            this._manager.TriggerJob(key);
        }

        private bool _disposed = false;

        /// <summary>
        /// 关闭所有任务，请关闭task manager
        /// </summary>
        public void Dispose() => this.Dispose(false);

        public void Dispose(bool wait)
        {
            if (this._disposed) { return; }
            this._manager?.ShutdownIfStarted_(wait);
            this._disposed = true;
        }
    }
}
