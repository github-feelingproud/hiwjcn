using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

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
            //
        }
    }
}
