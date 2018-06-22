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
using Lib.core;

namespace Lib.task
{
    public class TaskContainer : IDisposable
    {
        //延迟加载
        private readonly Lazy_<IScheduler> _lazy = new Lazy_<IScheduler>(() => 
        Task.Run(TaskHelper.GetDefaultScheduler).Result);

        public IScheduler TaskScheduler => this._lazy.Value;

        public async Task AddJobFromAssembly(params Assembly[] ass)
        {
            var jobs = ass.FindAllJobsAndCreateInstance_();
            foreach (var job in jobs.Where(x => x.AutoStart))
            {
                await this.TaskScheduler.AddJob_(job);
            }
        }

        /// <summary>
        /// 开启
        /// </summary>
        public async Task Start() => await this.TaskScheduler.StartIfNotStarted_();

        /// <summary>
        /// 关闭所有任务，请关闭task manager
        /// </summary>
        public void Dispose() => this.Dispose(false);

        private readonly object _lock = new object();

        public void Dispose(bool wait)
        {
            if (this._lazy.IsValueCreated)
            {
                lock (this._lock)
                {
                    if (this._lazy.IsValueCreated)
                    {
                        Task.Run(async () => await this.TaskScheduler.ShutdownIfStarted_(wait)).Wait();
                        this._lazy.Dispose();
                    }
                }
            }
        }
    }
}
