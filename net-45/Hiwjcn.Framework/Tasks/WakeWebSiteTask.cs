using Lib.extension;
using Lib.net;
using Lib.task;
using Quartz;
using System;
using System.Threading.Tasks;

namespace Hiwjcn.Framework.Tasks
{
    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution]
    public class WakeWebSiteTask : QuartzJobBase
    {
        public override string Name
        {
            get
            {
                return "唤醒网站";
            }
        }

        public override bool AutoStart
        {
            get
            {
                return true;
            }
        }

        public override ITrigger Trigger
        {
            get
            {
                return this.TriggerIntervalInMinutes(3);
            }
        }

        public override async Task Execute(IJobExecutionContext context)
        {
            var wakeuplist = new string[]
            {
                "http://www.qq.com"
            };

            foreach (var url in wakeuplist)
            {
                try
                {
                    var client = Lib.net.HttpClientManager.Instance.DefaultClient;
                    using (var response = await client.GetAsync(url))
                    {
                        var html = await response.Content.ReadAsStringAsync();
                        $"唤醒网站{url}，读取内容长度为{html?.Length}".AddBusinessInfoLog();
                    }
                }
                catch (Exception e)
                {
                    e.AddErrorLog($"{this.Name}:{url}");
                }
            }
        }
    }
}
