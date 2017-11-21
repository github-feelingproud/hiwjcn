using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Lib.distributed.zookeeper;
using Polly;

namespace Lib.distributed.zookeeper.ServiceManager
{
    public class ServiceRegister : ServiceManageBase
    {
        public ServiceRegister(string host) : base(host)
        {
            //
        }

        public async Task RegisterService(params Assembly[] ass)
        {
            using (var lk = new ZooKeeperDistributedLock(this._host, "/QPL/LOCK", "service_name"))
            {
                await this.RetryAsync().ExecuteAsync(async () => await lk.LockOrThrow());

                //do soemthing

                await lk.ReleaseLock();
            }
        }
    }
}
