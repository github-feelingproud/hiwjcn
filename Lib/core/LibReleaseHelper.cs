using Lib.data;
using Lib.extension;
using Lib.ioc;
using Lib.mq;
using Lib.task;
using System;

namespace Lib.core
{
    /// <summary>
    /// 释放Lib库内所用占用的资源
    /// </summary>
    public static class LibReleaseHelper
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

            try
            {
                //关闭rabbitmq
                ElasticsearchHelper.Dispose();
            }
            catch (Exception e)
            {
                e.AddErrorLog();
            }
        }
    }
}
