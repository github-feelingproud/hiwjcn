using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polly;

namespace Lib.distributed.zookeeper.ServiceManager
{
    public static class ServiceManageHelper
    {
        public static Policy RetryPolicy() =>
            Policy.Handle<Exception>().WaitAndRetry(3, i => TimeSpan.FromMilliseconds(i * 100));
    }
}
