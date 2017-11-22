using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Lib.distributed.zookeeper;
using Polly;
using Lib.rpc;
using org.apache.zookeeper;
using Lib.extension;

namespace Lib.distributed.zookeeper.ServiceManager
{
    public class ServiceRegister : ServiceManageBase
    {
        public ServiceRegister(string host) : base(host)
        {
            //
        }

        public async Task RegisterService(string base_url, string node_id, params Assembly[] ass)
        {
            var now = DateTime.Now;
            var list = new List<AddressModel>();
            foreach (var a in ass)
            {
                foreach (var t in a.FindContracts())
                {
                    var attr = t.GetCustomAttributes_<IsWcfContractAttribute>().FirstOrDefault() ??
                        throw new Exception("不应该出现的错误");
                    var model = new AddressModel()
                    {
                        Id = node_id,
                        Url = base_url + attr.RelativePath,
                        ServiceNodeName = ServiceManageHelper.ParseServiceName(t),
                        EndpointNodeName = $"node_{node_id}",
                        OnLineTime = now,
                    };
                }
            }
            foreach (var m in list)
            {
                var service_path = this._base_path + "/" + m.ServiceNodeName;
                await this.Client.CreatePersistentPathIfNotExist_(service_path);

                var path = service_path + "/" + m.EndpointNodeName;
                var data = m.ToJson().GetBytes(this._encoding);
                if (await this.Client.ExistAsync_(path))
                {
                    await this.Client.SetDataAsync_(path, data);
                }
                else
                {
                    await this.Client.CreateNode_(path, CreateMode.EPHEMERAL, data);
                }
            }
            /*
             
            using (var lk = new ZooKeeperDistributedLock(this._host, "/QPL/LOCK", "service_name"))
            {
                await this.RetryAsync().ExecuteAsync(async () => await lk.LockOrThrow());

                //do soemthing

                await lk.ReleaseLock();
            }
             */
        }
    }
}
