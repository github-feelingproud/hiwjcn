using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.helper;
using Quartz;
using Quartz.Impl;

namespace Lib.task
{
    public static class TaskHelper
    {
        public static IEnumerable<DateTime> CronIter(DateTime start, DateTime end, string cron_string)
        {
            var cron = new CronExpression(cron_string);

            var time = start;
            while (time <= end)
            {
                var next = cron.GetNextValidTimeAfter(time);
                if (next == null) { break; }
                time = next.Value.DateTime;
                yield return time;
            }
        }

        public static JobKey CreateJobKey(string name, string group = null)
        {
            name = name ?? throw new ArgumentNullException("job key name is null");
            return ValidateHelper.IsPlumpString(group) ?
                new JobKey(name, group) :
                new JobKey(name);
        }

        public static async Task<IScheduler> GetDefaultScheduler() =>
            await StdSchedulerFactory.GetDefaultScheduler();
    }
}
