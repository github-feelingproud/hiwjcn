using Lib.data;
using Lib.distributed;
using Lib.events;
using Lib.extension;
using Lib.ioc;
using Lib.mq;
using Lib.task;
using Lib.net;
using System;
using Lib.distributed.akka;
using Lib.mq.rabbitmq;
using Lib.distributed.redis;
using Lib.data.elasticsearch;
using Lib.rpc;

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
                //startup tasks
                LibStartUpHelper.Dispose();
            }
            catch (Exception e)
            {
                e.AddErrorLog();
            }

            try
            {
                //wcf service host
                ServiceHostManager.Host.Dispose();
            }
            catch (Exception e)
            {
                e.AddErrorLog();
            }

            try
            {
                //akka system
                AkkaSystemManager.Instance.Dispose();
            }
            catch (Exception e)
            {
                e.AddErrorLog();
            }
            
            try
            {
                //redis
                RedisClientManager.Instance?.Dispose();
            }
            catch (Exception e)
            {
                e.AddErrorLog();
            }

            try
            {
                //关闭rabbitmq
                RabbitMQClientManager.Instance?.Dispose();
            }
            catch (Exception e)
            {
                e.AddErrorLog();
            }

            try
            {
                //关闭ES搜索
                ElasticsearchClientManager.Instance?.Dispose();
            }
            catch (Exception e)
            {
                e.AddErrorLog();
            }

            try
            {
                //zookeeper
                //ZooKeeperClientManager.Instance?.Dispose();
            }
            catch (Exception e)
            {
                e.AddErrorLog();
            }

            try
            {
                //IOC
                AutofacIocContext.Instance.Dispose();
            }
            catch (Exception e)
            {
                e.AddErrorLog();
            }

            try
            {
                //httpclient
                HttpClientManager.Instance.Dispose();
            }
            catch (Exception e)
            {
                e.AddErrorLog();
            }

            //回收内存
            GC.Collect();
        }
    }
}
