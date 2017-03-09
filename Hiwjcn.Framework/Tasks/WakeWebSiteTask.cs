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
using Lib.task;

namespace Hiwjcn.Framework.Tasks
{
    public class WakeWebSiteTask : QuartzJobBase
    {
        public override string Name
        {
            get
            {
                return "唤醒网站";
            }
        }

        public override bool Start
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
                return TaskManager.BuildCommonTrigger(15);
            }
        }

        public override void Execute(IJobExecutionContext context)
        {
            try
            {
                var url = "http://colin.hiwj.cn/";
                var html = HttpClientHelper.Get(url);
                $"唤醒网站，读取内容长度为{html?.Length}".AddBusinessInfoLog();
            }
            catch (Exception e)
            {
                e.AddLog(this.GetType());
            }
        }
    }
}
