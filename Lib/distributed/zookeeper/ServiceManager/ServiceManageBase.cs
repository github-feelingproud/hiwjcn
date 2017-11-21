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
    /// <summary>
    /// /QPL/WCF/ORDER/m-1
    /// /QPL/WCF/ORDER/m-2
    /// /QPL/WCF/ORDER/m-3
    /// /QPL/WCF/ORDER/m-4
    /// </summary>
    public abstract class ServiceManageBase : AlwaysOnZooKeeperClient
    {
        protected readonly string base_path;

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
                this.Retry().Execute(() => this.InitBasePath());
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
                await client.CreatePersistentPathIfNotExist_(this.base_path);
            }).Wait();
        }

        protected Policy Retry() => ServiceManageHelper.RetryPolicy();

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
