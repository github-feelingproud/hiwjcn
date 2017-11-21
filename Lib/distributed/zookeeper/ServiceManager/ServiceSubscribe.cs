using System;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Threading;
using org.apache.zookeeper;
using Lib.extension;
using Lib.helper;
using Lib.data;
using Lib.ioc;
using Lib.core;
using System.Threading.Tasks;
using static org.apache.zookeeper.ZooDefs;
using org.apache.zookeeper.data;
using System.Net;
using System.Net.Http;
using Lib.net;
using Lib.rpc;
using Lib.distributed.zookeeper.watcher;
using Polly;

namespace Lib.distributed.zookeeper.ServiceManager
{
    public class ServiceSubscribe : ServiceManageBase
    {
        private readonly Watcher _watcher;
        private readonly Dictionary<string, string> _endpoints = new Dictionary<string, string>();

        public ServiceSubscribe(string host) : base(host)
        {
            _watcher = new CallBackWatcher(e =>
            {
                return this.NodeChanges(e);
            });

            this.StartWatch();
            this.Connected += () => this.StartWatch();
        }

        private void StartWatch()
        {
            try
            {
                this.Retry().Execute(() =>
                {
                    var client = this.GetClientManager();
                    Task.Factory.StartNew(async () =>
                    {
                        await this.NodeChildrenChanged(this.base_path);
                    }).Wait();
                });
            }
            catch (Exception e)
            {
                throw new Exception("订阅服务节点失败", e);
            }
        }

        private async Task NodeChildrenChanged(string path)
        {
            try
            {
                var client = this.GetClientManager();
                var children = await client.Client.GetChildrenOrThrow_(path, this._watcher);
                foreach (var child in children)
                {
                    Console.WriteLine(child);
                }
            }
            catch (Exception e)
            {
                e.AddErrorLog();
            }
        }

        private async Task NodeChanges(WatchedEvent e)
        {
            var event_type = e.get_Type();
            var path = e.getPath();
            if (path != this.base_path)
            {
                return;
            }

            switch (event_type)
            {
                case Watcher.Event.EventType.NodeChildrenChanged:
                    //注册节点发生改变
                    await this.NodeChildrenChanged(path);
                    break;
                default:
                    break;
            }
        }

    }
}
