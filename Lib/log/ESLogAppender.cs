using Lib.data;
using log4net.Appender;
using log4net.Core;
using System;
using Lib.extension;
using System.Collections.Generic;
using Nest;
using Lib.helper;
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
            Policy.Handle<Exception>().CircuitBreaker(10, TimeSpan.FromMinutes(1));

        public const string IndexName = "lib_es_log_index";

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

                    client.CreateIndexIfNotExists(IndexName);
                });
            }
            catch
            { }
        }

        protected override void SendBuffer(LoggingEvent[] events)
        {
            try
            {
                p.Execute(() =>
                {
                    var pool = ElasticsearchClientManager.Instance.DefaultClient;
                    var client = new ElasticClient(pool);

                    client.AddToIndex(IndexName, events);
                });
            }
            catch
            { }
        }
    }
}
