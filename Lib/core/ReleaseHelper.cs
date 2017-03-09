using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.extension;
using Lib.data;
using Lib.ioc;
using Lib.mq;
using Lib.task;

namespace Lib.core
{
    public static class ReleaseHelper
    {
        public static void DisposeAll()
        {
            try
            {
                //task
                TaskManager.StopTasks();
            }
            catch (Exception e)
            {
                e.AddErrorLog();
            }

            try
            {
                //redis
                RedisConnectionManager.Dispose();
            }
            catch (Exception e)
            {
                e.AddErrorLog();
            }

            try
            {
                //IOC
                AppContext.Dispose();
            }
            catch (Exception e)
            {
                e.AddErrorLog();
            }

            try
            {
                //关闭rabbitmq
                RabbitMQClient.DefaultClient.Dispose();
            }
            catch (Exception e)
            {
                e.AddErrorLog();
            }
        }
    }
}
