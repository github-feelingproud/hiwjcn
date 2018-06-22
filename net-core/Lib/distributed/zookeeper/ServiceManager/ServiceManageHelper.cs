using Lib.extension;
using Polly;
using System;

namespace Lib.distributed.zookeeper.ServiceManager
{
    public static class ServiceManageHelper
    {
        public static Policy RetryPolicy() =>
            Policy.Handle<Exception>().WaitAndRetry(3, i => TimeSpan.FromMilliseconds(i * 100));

        public static Policy RetryAsyncPolicy() =>
            Policy.Handle<Exception>().WaitAndRetryAsync(3, i => TimeSpan.FromMilliseconds(i * 100));

        public static string ParseServiceName<T>() => ParseServiceName(typeof(T));

        public static string ParseServiceName(Type t) => $"{t.FullName}".RemoveWhitespace();

        public static string EndpointNodeName(string node_id) => $"node_{node_id}";
    }
}
