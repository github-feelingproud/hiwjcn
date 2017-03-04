using Lib.core;
using Lib.helper;
using Lib.net;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lib.extension;

namespace Hiwjcn.Framework.Tasks
{
    public class WakeWebSiteTask : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            try
            {
                Thread.Sleep(1000 * 60);
                var url = "http://colin.hiwj.cn/";
                HttpClientHelper.SendHttpRequest(url, null, null, null, RequestMethodEnum.GET, 60, (res) =>
                 {
                     var html = AsyncHelper.RunSync(() => res.Content.ReadAsStringAsync());
                     LogHelper.Info(this.GetType(), $"唤醒网站，读取内容长度为{html?.Length}");
                 });
            }
            catch (Exception e)
            {
                e.AddLog(this.GetType());
            }
        }
    }
}
