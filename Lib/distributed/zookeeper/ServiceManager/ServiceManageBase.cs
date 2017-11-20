using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.distributed;
using Lib.distributed.zookeeper;
using System.Reflection;
using Lib.extension;
using Polly;

namespace Lib.distributed.zookeeper.ServiceManager
{
    public class ServiceManageBase : AlwaysOnZooKeeperClient
    {
        private readonly string base_path;

        public ServiceManageBase(string host) : this(host, "/QPL/WCF") { }

        public ServiceManageBase(string host, string path) : base(host)
        {
            this.base_path = path ?? throw new ArgumentNullException(path);
            if (!this.base_path.StartsWith("/") || this.base_path.EndsWith("/"))
            {
                throw new Exception("path必须以/开头，并且不能以/结尾");
            }

            try
            {
                Policy.Handle<Exception>().Retry(3).Execute(() => this.InitBasePath());
            }
            catch (Exception e)
            {
                throw new Exception("尝试创建服务注册base path失败", e);
            }
        }

        protected void InitBasePath()
        {
            var client = this.GetClientManager();
            Task.Factory.StartNew(async () =>
            {
                if (!await client.Client.ExistAsync_(this.base_path))
                {
                    await client.Client.CreatePersistentPathIfNotExist_(this.base_path);
                }
            });
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
