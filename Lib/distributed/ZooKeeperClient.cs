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

namespace Lib.distributed
{
    public class EmptyWatcher : Watcher
    {
        public override async Task process(WatchedEvent @event)
        {
            await Task.FromResult(1);
        }
    }

    public class CallBackWatcher : Watcher
    {
        private readonly Func<WatchedEvent, Task> callback;

        public CallBackWatcher(Func<WatchedEvent, Task> callback)
        {
            this.callback = callback ?? throw new ArgumentNullException(nameof(callback));
        }

        public override async Task process(WatchedEvent @event)
        {
            await this.callback.Invoke(@event);
        }
    }

    /// <summary>
    /// 
    /// 资料：邮箱搜索“zookeeper资料”
    /// </summary>
    public class ZooKeeperClient : SerializeBase, IDisposable
    {
        protected readonly ZooKeeper _zookeeper;

        public ZooKeeperClient(string configurationName) : this(ZooKeeperConfigSection.FromSection(configurationName))
        {
            //
        }

        public ZooKeeperClient(ZooKeeperConfigSection configuration) :
            this(configuration.Server, TimeSpan.FromMilliseconds(configuration.SessionTimeOut), null)
        {
            //
        }

        public ZooKeeperClient(string host, TimeSpan timeout, Watcher watcher)
        {
            this._zookeeper = new ZooKeeper(host, (int)timeout.TotalMilliseconds, watcher ?? new EmptyWatcher());
        }

        public virtual async Task WatchedChange(WatchedEvent @event)
        {
            await Task.FromResult(1);
        }

        public bool IsAlive
        {
            get => this._zookeeper?.getState() == ZooKeeper.States.CONNECTED;
        }

        public ZooKeeper Client
        {
            get => this._zookeeper;
        }

        public void Dispose()
        {
            try
            {
                if (this._zookeeper != null)
                {
                    Task.Factory.StartNew(async () => await this._zookeeper.closeAsync()).Wait();
                }
            }
            catch (Exception e)
            {
                e.AddErrorLog();
            }
        }
    }

    public class AlwaysOnZooKeeperClient : Watcher, IDisposable
    {
        private readonly string Host;

        private readonly string ServicePath;

        private ZooKeeperClient _client;

        public event Action OnRecconected;
        private bool IsClosing = false;

        private readonly ManualResetEvent _event_set = new ManualResetEvent(false);

        public AlwaysOnZooKeeperClient(string host) : this(host, "/QPL/WCF")
        { }

        public AlwaysOnZooKeeperClient(string host, string service_path)
        {
            this.Host = host ?? throw new ArgumentNullException(nameof(host));
            this.ServicePath = service_path ?? throw new ArgumentNullException(nameof(service_path));

            this.CreateNewClient();

            Task.Factory.StartNew(async () => await this.Init()).Wait();
        }

        private async Task Init()
        {
            await this.UseClient(async client =>
            {
                if (!await client.Client.ExistAsync_(this.ServicePath))
                {
                    await client.Client.CreatePersistentPathIfNotExist(this.ServicePath);
                }
            });
        }

        private void CreateNewClient()
        {
            try
            {
                this._event_set.Set();

                if (this._client != null)
                {
                    this.CloseClient();
                }

                if (this._client == null)
                {
                    this.IsClosing = false;
                    this._client = new ZooKeeperClient(this.Host, TimeSpan.FromSeconds(5), this);
                }
            }
            finally
            {
                this._event_set.Reset();
            }
        }

        private async Task UseClient(Func<ZooKeeperClient, Task> action)
        {
            try
            {
                this._event_set.Set();
                if (this._client == null) { throw new Exception("zookeeper client is not prepared"); }

                await this.EnsureClient();
                await action.Invoke(this._client);
            }
            finally
            {
                this._event_set.Reset();
            }
        }

        public async Task FetchData()
        {
            await this.UseClient(async client =>
            {
                if (await client.Client.ExistAsync_(this.ServicePath))
                {
                    var child = await client.Client.getChildrenAsync(this.ServicePath);
                    Console.WriteLine($"找到节点{",".Join_(child.Children)}");
                }
                else
                {
                    Console.WriteLine("不存在节点");
                }
            });
        }

        private async Task EnsureClient()
        {
            var start = DateTime.Now;
            while (this._client == null)
            {
                if ((DateTime.Now - start).TotalSeconds > 5)
                {
                    throw new Exception("等待可用链接超时");
                }
                await Task.Delay(5);
            }
        }

        private void CloseClient()
        {
            this.IsClosing = true;
            this._client?.Dispose();
            this._client = null;
        }

        public override async Task process(WatchedEvent @event)
        {
            if (this.IsClosing) { return; }

            var event_type = @event.get_Type();
            var zk_status = @event.getState();
            if (zk_status == Event.KeeperState.SyncConnected)
            {
                //
            }
            else
            {
                this.CloseClient();
                this.CreateNewClient();
                this.OnRecconected.Invoke();
                $"创建新的ZK客户端".AddBusinessInfoLog();
            }
            await Task.FromResult(1);
        }

        public void Dispose()
        {
            this.CloseClient();
            this._event_set.Dispose();
        }
    }

    public static class ZooKeeperClientExtension
    {
        public static async Task<bool> ExistAsync_(this ZooKeeper client, string path) =>
            await client.existsAsync(path) != null;

        public static async Task<string> CreatePersistentPathIfNotExist(this ZooKeeper client,
            string path, byte[] data = null)
        {
            if (await client.ExistAsync_(path))
            {
                return path;
            }

            var sp = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var p = string.Empty;
            foreach (var itm in sp)
            {
                p += $"/{itm}";
                if (await client.ExistAsync_(p))
                {
                    continue;
                }

                await client.createAsync(p, data, Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT);
            }
            return "/" + "/".Join_(sp);
        }

        public static async Task<string> CreateSequential(this ZooKeeper client, string path,
            byte[] data = null, bool persistent = true)
        {
            var p = await client.createAsync(path, data,
                Ids.OPEN_ACL_UNSAFE,
                persistent ? CreateMode.PERSISTENT_SEQUENTIAL : CreateMode.EPHEMERAL_SEQUENTIAL);
            return p.Substring(path.Length);
        }

        public static async Task<Stat> SetDataAsync<T>(this ZooKeeper client, string path, T data) =>
            await client.setDataAsync(path, data.ToJson().GetBytes());

        public static async Task DeleteNodeRecursively_(this ZooKeeper client, string path)
        {
            var handlered_list = new List<string>();

            List<string> Sp_path(string _p) =>
                _p.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            async Task __DeleteNode(string pre_path, string p)
            {
                var node_sp = Sp_path(pre_path);
                var node_path = Sp_path(p);
                if (!ValidateHelper.IsPlumpList(node_path))
                {
                    throw new Exception($"不能删除：{p}");
                }
                node_sp.AddRange(node_path);

                var current_node = "/" + "/".Join(node_sp);
                //检查死循环
                handlered_list.AddOnceOrThrow(current_node,
                    $"递归发生错误，已处理节点：{handlered_list.ToJson()}，再次处理：{current_node}");

                if (!await client.ExistAsync_(current_node))
                {
                    return;
                }
                var res = await client.getChildrenAsync(current_node, false);
                if (ValidateHelper.IsPlumpList(res.Children))
                {
                    foreach (var child in res.Children.Where(x => ValidateHelper.IsPlumpString(x)))
                    {
                        //递归
                        await __DeleteNode(current_node, child);
                    }
                }
                await client.deleteAsync(current_node);
            }

            //入口
            await __DeleteNode(string.Empty, path);
        }

        public static async Task DeleteSingleNode_(this ZooKeeper client, string path) =>
            await client.deleteAsync(path);
    }

    /// <summary>
    /// zookeeper配置
    /// </summary>
    public class ZooKeeperConfigSection : ConfigurationSection
    {
        public static ZooKeeperConfigSection FromSection(string name)
        {
            return (ZooKeeperConfigSection)ConfigurationManager.GetSection(name);
        }

        [ConfigurationProperty(nameof(SessionTimeOut), IsRequired = true)]
        public int SessionTimeOut
        {
            get { return int.Parse(this[nameof(SessionTimeOut)].ToString()); }
        }

        [ConfigurationProperty(nameof(Server), IsRequired = true)]
        public string Server
        {
            get { return this[nameof(Server)].ToString(); }
        }

        [ConfigurationProperty(nameof(MasterSlavePath), IsRequired = false)]
        public string MasterSlavePath
        {
            get { return this[nameof(MasterSlavePath)].ToString(); }
        }

        [ConfigurationProperty(nameof(DistributedLockPath), IsRequired = false)]
        public string DistributedLockPath
        {
            get { return this[nameof(DistributedLockPath)].ToString(); }
        }
    }
}
