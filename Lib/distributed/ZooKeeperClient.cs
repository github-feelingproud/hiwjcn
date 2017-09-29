using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Threading;
using Org.Apache.Zookeeper.Data;
using ZooKeeperNet;
using Lib.extension;
using Lib.helper;
using Lib.data;
using Lib.ioc;
using Lib.core;

namespace Lib.distributed
{
    public class DefaultWatcher : IWatcher
    {
        public void Process(WatchedEvent @event)
        {
            //
        }
    }

    /// <summary>
    /// 
    /// 资料：邮箱搜索“zookeeper资料”
    /// </summary>
    public class ZooKeeperClient : SerializeBase, IDisposable
    {
        private readonly IZooKeeper _zookeeper;

        //private readonly ManualResetEvent _resetEvent = new ManualResetEvent(false);

        private readonly int ConnectionLossTimeout;

        public ZooKeeperClient(string configurationName) : this(ZooKeeperConfigSection.FromSection(configurationName)) { }

        public ZooKeeperClient(ZooKeeperConfigSection configuration, IWatcher watcher = null)
        {
            if (!ValidateHelper.IsPlumpString(configuration.Server))
            {
                throw new ArgumentNullException("zookeeper server can not be empty");
            }

            this.ConnectionLossTimeout = configuration.SessionTimeOut;
            var timeOut = TimeSpan.FromMilliseconds(configuration.SessionTimeOut);

            _zookeeper = new ZooKeeper(
                configuration.Server,
                timeOut,
                watcher ?? new DefaultWatcher());
        }

        public bool IsAlive
        {
            get
            {
                if (_zookeeper == null) { return false; }
                return _zookeeper.State.IsAlive();
            }
        }

        public IZooKeeper Client { get => this._zookeeper; }

        public T Invoke<T>(Func<IZooKeeper, T> func) => func.Invoke(_zookeeper);

        public void Invoke(Action<IZooKeeper> action) => this.Invoke(x => { action.Invoke(x); return true; });

        public virtual T Get<T>(string path, bool watch = false)
        {
            return Deserialize<T>(this.Invoke(x => x.GetData(path, watch, null)));
        }

        public virtual bool Set<T>(string path, T objData, int version = -1)
        {
            var buffer = Serialize(objData);
            return this.Invoke(x => x.SetData(path, buffer, version)) != null;
        }

        public IEnumerable<string> GetChildren(string path, bool watch = false)
        {
            return this.Invoke(x => x.GetChildren(path, watch));
        }

        public bool CreatePersistentPath(string path)
        {
            if (this.Invoke(x => x.Exists(path, false)) != null)
                return false;

            var _path = string.Empty;
            foreach (var item in path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries))
            {
                _path = _path + "/" + item;
                if (this.Invoke(x => x.Exists(_path, false)) != null)
                {
                    continue;
                }

                try
                {
                    this.Invoke(x => x.Create(_path, null, Ids.OPEN_ACL_UNSAFE, CreateMode.Persistent));
                }
                catch (KeeperException.NodeExistsException e)
                {
                    e.AddErrorLog();
                }
            }

            return true;
        }

        public string CreateSequential(string path, bool persistent) => CreateSequential(path, null, persistent);

        public string CreateSequential(string path, byte[] data, bool persistent)
        {
            return this.Invoke(x =>
                    x.Create(path,
                    data,
                    Ids.OPEN_ACL_UNSAFE,
                    persistent ? CreateMode.PersistentSequential : CreateMode.EphemeralSequential)).Substring(path.Length);
        }

        public void DeleteNode(string path) => this.Invoke(x => x.Delete(path, -1));

        public Stat Watch(string path) => this.Invoke(x => x.Exists(path, true));

        public Stat Watch(string path, IWatcher watcher) => this.Invoke(x => x.Exists(path, watcher));

        public void Dispose()
        {
            this._zookeeper?.Dispose();
            //_resetEvent.Dispose();
        }
    }

    /// <summary>
    /// 链接管理
    /// </summary>
    public class ZooKeeperClientManager : StaticClientManager<ZooKeeperClient>
    {
        public static readonly ZooKeeperClientManager Instance = new ZooKeeperClientManager();

        public override string DefaultKey
        {
            get
            {
                return "lib_zookeeper";
            }
        }

        public override bool CheckClient(ZooKeeperClient ins)
        {
            return ins != null && ins.IsAlive;
        }

        public override ZooKeeperClient CreateNewClient(string key)
        {
            var config = ZooKeeperConfigSection.FromSection(key);
            return new ZooKeeperClient(config);
        }

        public override void DisposeClient(ZooKeeperClient ins)
        {
            ins?.Dispose();
        }
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
