using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Lib.extension;
using System.Reflection;
using Quartz.Impl.Matchers;
using Lib.helper;

namespace Lib.task
{
    /// <summary>
    /// quartz扩展
    /// </summary>
    public static class QuartzExtension
    {
        /// <summary>
        /// 获取task信息
        /// </summary>
        /// <returns></returns>
        public static async Task<List<ScheduleJobModel>> GetAllTasks_(this IScheduler manager)
        {
            //所有任务
            var jobKeys = await manager.GetAllJobKeys_();
            //正在运行的任务
            var runningJobs = await manager.GetCurrentlyExecutingJobs();

            var list = new List<ScheduleJobModel>();
            foreach (var jobKey in jobKeys)
            {
                var triggers = await manager.GetTriggersOfJob(jobKey);
                if (!ValidateHelper.IsPlumpList(triggers)) { continue; }
                foreach (var trigger in triggers)
                {
                    var job = new ScheduleJobModel()
                    {
                        JobName = jobKey.Name,
                        JobGroup = jobKey.Group,
                        TriggerName = trigger.Key.Name,
                        TriggerGroup = trigger.Key.Group,
                        StartTime = trigger.StartTimeUtc.LocalDateTime,
                        PreTriggerTime = trigger.GetPreviousFireTimeUtc()?.LocalDateTime,
                        NextTriggerTime = trigger.GetNextFireTimeUtc()?.LocalDateTime,
                        JobStatus = (await manager.GetTriggerState(trigger.Key)).GetTriggerState(),
                        IsRunning = runningJobs.Any(x => x.JobDetail.Key == jobKey)
                    };

                    list.Add(job);
                }
            }
            return list;
        }

        public static async Task<IEnumerable<JobKey>> GetAllJobKeys_(this IScheduler manager) =>
            (await manager.GetJobKeys(GroupMatcher<JobKey>.AnyGroup())).AsEnumerable();

        public static async Task AddJob_(this IScheduler manager, QuartzJobBase job, bool throw_if_exist = true) =>
            await manager.AddJob_(job.GetType(), job.CachedTrigger, job.Name, job.Group, throw_if_exist);

        public static async Task AddJob_(this IScheduler manager,
            Type t, ITrigger trigger, string name, string group = null, bool throw_if_exist = true)
        {
            Com.AssertNotNull(t, nameof(t));
            Com.AssertNotNull(t, nameof(trigger));
            Com.AssertNotNull(t, nameof(name));

            var builder = JobBuilder.Create(t);
            if (ValidateHelper.IsPlumpString(group))
            {
                builder = builder.WithIdentity(name, group);
            }
            else
            {
                builder = builder.WithIdentity(name);
            }
            var job = builder.Build();

            if ((await manager.GetAllJobKeys_()).Contains(job.Key))
            {
                if (throw_if_exist)
                {
                    throw new Exception("job已经存在");
                }
                else
                {
                    return;
                }
            }

            await manager.ScheduleJob(job, trigger);
        }

        public static async Task StartIfNotStarted_(this IScheduler manager, TimeSpan? delay = null)
        {
            if (!manager.IsStarted)
            {
                if (delay == null)
                {
                    await manager.Start();
                }
                else
                {
                    await manager.StartDelayed(delay.Value);
                }
            }
        }

        /// <summary>
        /// 如果任务开启就关闭
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="waitForJobsToComplete"></param>
        public static async Task ShutdownIfStarted_(this IScheduler manager, bool waitForJobsToComplete = false)
        {
            if (!waitForJobsToComplete)
            {
                $"任务关闭不会等待任务完成，肯能导致数据不完整，你可以设置{nameof(waitForJobsToComplete)}来调整".AddBusinessWarnLog();
            }
            if (manager.IsStarted)
            {
                await manager.Shutdown(waitForJobsToComplete);
            }
        }

        /// <summary>
        /// 找到任务
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Type[] FindJobTypes_(this Assembly a)
        {
            return a.GetTypes().Where(x => x.IsNormalClass() && x.IsAssignableTo_<QuartzJobBase>()).ToArray();
        }

        public static List<QuartzJobBase> FindAllJobsAndCreateInstance_(this IEnumerable<Assembly> ass)
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

            return jobs;
        }

        /// <summary>
        /// 获取状态的描述
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static string GetTriggerState(this TriggerState state)
        {
            switch (state)
            {
                case TriggerState.Blocked:
                    return "阻塞";
                case TriggerState.Complete:
                    return "完成";
                case TriggerState.Error:
                    return "错误";
                case TriggerState.None:
                    return "无状态";
                case TriggerState.Normal:
                    return "正常";
                case TriggerState.Paused:
                    return "暂停";
                default:
                    return state.ToString();
            }
        }
    }
}
