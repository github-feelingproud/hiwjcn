using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using Lib.distributed.zookeeper;
using Polly;
using Lib.rpc;
using Lib.helper;
using org.apache.zookeeper;
using Lib.extension;
using System.ServiceModel;

namespace Lib.distributed.zookeeper.ServiceManager
{
    /// <summary>
    /// 应该作为静态类
    /// </summary>
    public class ServiceRegister : ServiceManageBase
    {
        private readonly string _base_url;
        private readonly string _node_id;
        private readonly IReadOnlyList<Assembly> _ass;

        public ServiceRegister(string host, string service_base_url, Assembly[] ass) : base(host)
        {
            this._base_url = service_base_url ?? throw new ArgumentNullException(nameof(service_base_url));
            this._ass = (ass ?? throw new ArgumentNullException(nameof(ass))).ToList().AsReadOnly();
            if (this._ass.Count <= 0) { throw new Exception("至少注册一个程序集"); }

            this._node_id = this.Client.getSessionId().ToString();

            this.Retry().Execute(() => this.Reg());
            this.OnRecconected += () => this.Reg();
        }

        public void Reg() => AsyncHelper_.RunSync(() => this.RegisterService());

        private async Task RegisterService()
        {
            var list = new List<AddressModel>();

            foreach (var a in this._ass)
            {
                var contractImpl = a.FindServiceContractsImpl();

                foreach (var t in contractImpl)
                {
                    var attr = t.GetCustomAttributes_<ServiceContractImplAttribute>().First();
                    foreach (var contract in t.FindServiceContracts())
                    {
                        var model = new AddressModel()
                        {
                            Url = this._base_url + attr.RelativePath,
                            ServiceNodeName = ServiceManageHelper.ParseServiceName(contract),
                            EndpointNodeName = ServiceManageHelper.EndpointNodeName(this._node_id),
                        };
                        list.Add(model);
                    }
                }
            }

            foreach (var m in list)
            {
                var service_path = this._base_path + "/" + m.ServiceNodeName;
                await this.Client.EnsurePath(service_path);

                var path = service_path + "/" + m.EndpointNodeName;
                var data = this._serializer.Serialize(m);
                if (await this.Client.ExistAsync_(path))
                {
                    await this.Client.SetDataAsync_(path, data);
                }
                else
                {
                    //创建临时节点，服务端下线自动删除
                    await this.Client.CreateNode_(path, CreateMode.EPHEMERAL, data);
                }
            }
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
