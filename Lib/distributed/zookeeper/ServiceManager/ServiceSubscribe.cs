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
using System.Text;

namespace Lib.distributed.zookeeper.ServiceManager
{
    public class ServiceSubscribe : ServiceManageBase
    {
        private readonly Watcher _children_watcher;
        private readonly Watcher _node_watcher;
        private readonly List<AddressModel> _endpoints = new List<AddressModel>();
        private readonly Random _ran = new Random((int)DateTime.Now.Ticks);

        public ServiceSubscribe(string host) : base(host)
        {
            this._children_watcher = new CallBackWatcher(e =>
            {
                return this.NodeChanges(e);
            });
            this._node_watcher = new CallBackWatcher(e =>
            {
                return this.NodeChanges(e);
            });

            this.StartWatch();
            this.OnConnected += () => this.StartWatch();
        }

        public IReadOnlyList<AddressModel> AllService() => this._endpoints.AsReadOnly();

        public AddressModel Resolve<T>()
        {
            var name = ServiceManageHelper.ParseServiceName<T>();
            var list = this._endpoints.Where(x => x.ServiceNodeName == name).ToList();
            if (ValidateHelper.IsPlumpList(list))
            {
                return this._ran.Choice(list);
            }
            return null;
        }

        public string ResolveSvc<T>() => this.Resolve<T>()?.Url;

        private void StartWatch()
        {
            try
            {
                this.Retry().Execute(() =>
                {
                    AsyncHelper_.RunSync(() => this.NodeChildrenChanged(this.base_path));
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
                var base_path_level = this.base_path.SplitZookeeperPath().Count;
                var path_level = path.SplitZookeeperPath().Count;
                if (path_level < base_path_level || path_level > base_path_level + 1) { return; }

                if (path_level == base_path_level)
                {
                    //qpl/wcf
                }
                else if (path_level == base_path_level + 1)
                {
                    //qpl/wcf/order
                }
                else
                {
                    $"不能处理的节点{path}".AddBusinessInfoLog();
                }

                var client = this.GetClientManager();
                var children = await client.GetChildrenOrThrow_(path, this._children_watcher);

                Console.WriteLine(children.AsSteps());

                foreach (var child in children)
                {
                    //
                }
            }
            catch (Exception e)
            {
                e.AddErrorLog();
            }
        }

        private async Task EndpointNodeChildrenChanged()
        { }

        private async Task NodeDataChanged(string path)
        {
            try
            {
                var client = this.GetClientManager();
                var bs = await client.GetDataOrThrow_(path, this._node_watcher);
                if (ValidateHelper.IsPlumpList(bs))
                {
                    var data = Encoding.UTF8.GetString(bs).JsonToEntity<List<AddressModel>>();

                    var parent = path.SplitZookeeperPath().Reverse_().Skip(1).Take(1).FirstOrDefault();

                    Console.WriteLine($"{parent}修改了数据：{data.ToJson()}");
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

            switch (event_type)
            {
                case Watcher.Event.EventType.NodeChildrenChanged:
                    //注册节点发生改变
                    await this.NodeChildrenChanged(path);
                    break;
                case Watcher.Event.EventType.NodeDataChanged:
                    //单个节点数据发生修改
                    await this.NodeDataChanged(path);
                    break;
                default:
                    break;
            }
        }

    }
}
