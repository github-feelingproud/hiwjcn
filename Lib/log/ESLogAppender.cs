using Lib.data;
using log4net.Appender;
using log4net.Core;
using System;
using Lib.extension;
using System.Collections.Generic;
using Nest;
using System.Diagnostics;
using Lib.helper;
using System.Linq;
using Polly;
using Polly.CircuitBreaker;

namespace Lib.log
{
    /// <summary>
    /// 使用redis存储日志
    /// https://github.com/lokki/RedisAppender
    /// </summary>
    public class ESLogAppender : BufferingAppenderSkeleton
    {
        private static readonly CircuitBreakerPolicy p =
            Policy.Handle<Exception>().CircuitBreaker(50, TimeSpan.FromMinutes(1));

        public ESLogAppender()
        {
            //
        }

        public override void ActivateOptions()
        {
            try
            {
                Policy.Handle<Exception>().WaitAndRetry(3, i => TimeSpan.FromSeconds(i)).Execute(() =>
                {
                    var pool = ElasticsearchClientManager.Instance.DefaultClient;
                    var client = new ElasticClient(pool);

                    client.CreateIndexIfNotExists(ESLogHelper.IndexName);
                });
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.GetInnerExceptionAsJson());
            }
        }

        protected override void SendBuffer(LoggingEvent[] events)
        {
            try
            {
                p.Execute(() =>
                {
                    Policy.Handle<Exception>().WaitAndRetry(3, i => TimeSpan.FromMilliseconds(i * 100)).Execute(() =>
                    {
                        var pool = ElasticsearchClientManager.Instance.DefaultClient;
                        var client = new ElasticClient(pool);

                        client.AddToIndex(ESLogHelper.IndexName, events.Select(x => new ESLogLine(x)).ToArray());
                    });
                });
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.GetInnerExceptionAsJson());
            }
        }
    }
}
