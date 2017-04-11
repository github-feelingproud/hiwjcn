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

        private readonly ManualResetEvent _resetEvent = new ManualResetEvent(false);

        public ZooKeeperClient(string configurationName) : this(ZooKeeperConfigSection.FromSection(configurationName)) { }

        public ZooKeeperClient(ZooKeeperConfigSection configuration)
        {
            if (!ValidateHelper.IsPlumpString(configuration.Server))
            {
                throw new ArgumentNullException("zookeeper server can not be empty");
            }

            ConnectionLossTimeout = configuration.SessionTimeOut;
            var timeOut = TimeSpan.FromMilliseconds(configuration.SessionTimeOut);

            if (AppContext.IsRegistered<IWatcher>())
            {
                _zookeeper = new ZooKeeper(
                    configuration.Server,
                    timeOut,
                    AppContext.GetObject<IWatcher>());
            }
            else
            {
                _zookeeper = new ZooKeeper(
                    configuration.Server,
                    timeOut,
                    new DefaultWatcher());
            }
        }

        protected int ConnectionLossTimeout { get; set; }

        public bool IsAlive
        {
            get
            {
                if (_zookeeper == null) { return false; }
                return _zookeeper.State.IsAlive();
            }
        }

        public T Invoke<T>(string path, Func<IZooKeeper, string, T> func)
        {
            if (_resetEvent.WaitOne(ConnectionLossTimeout))
            {
                return func(_zookeeper, path);
            }
            else
            {
                return func(_zookeeper, path);
            }
        }

        public void Invoke(string path, Action<IZooKeeper, string> action)
        {
            if (_resetEvent.WaitOne(ConnectionLossTimeout))
            {
                action(_zookeeper, path);
            }
            else
            {
                action(_zookeeper, path);
            }
        }

        public virtual T Get<T>(string path, bool watch = false)
        {
            return Deserialize<T>(Invoke(path, (zookeeper, p) => zookeeper.GetData(p, watch, null)));
        }

        public virtual bool Set<T>(string path, T objData, int version = -1)
        {
            var buffer = Serialize(objData);
            return Invoke(path, (zookeeper, p) => zookeeper.SetData(p, buffer, version)) != null;
        }

        public IEnumerable<string> GetChildren(string path, bool watch = false)
        {
            return Invoke(path, (zookeeper, p) => zookeeper.GetChildren(p, watch));
        }

        public bool CreatePersistentPath(string path)
        {
            if (Invoke(path, (zookeeper, p) => zookeeper.Exists(p, false)) != null)
                return false;

            var _path = string.Empty;
            foreach (var item in path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries))
            {
                _path = _path + "/" + item;
                if (Invoke(_path, (zookeeper, p) => zookeeper.Exists(p, false)) != null)
                {
                    continue;
                }

                try
                {
                    Invoke(_path, (zookeeper, p) => zookeeper.Create(p, null, Ids.OPEN_ACL_UNSAFE, CreateMode.Persistent));
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
            return Invoke(path, (zookeeper, p) =>
                zookeeper.Create(p,
                    data,
                    Ids.OPEN_ACL_UNSAFE,
                    persistent ? CreateMode.PersistentSequential : CreateMode.EphemeralSequential)).Substring(path.Length);
        }

        public void DeleteNode(string path) => Invoke(path, (zookeeper, p) => zookeeper.Delete(p, -1));

        public Stat Watch(string path) => Invoke(path, (zookeeper, p) => zookeeper.Exists(p, true));

        public Stat Watch(string path, IWatcher watcher) => Invoke(path, (zookeeper, p) => zookeeper.Exists(p, watcher));

        public bool _disposed = false;
        public void Dispose()
        {
            _zookeeper.Dispose();
            _resetEvent.Dispose();
            _disposed = true;
        }
    }

    /// <summary>
    /// 链接管理
    /// </summary>
    public class ZooKeeperClientManager : StaticClientManager<ZooKeeperClient>
    {
        public static readonly ZooKeeperClientManager Instance = new ZooKeeperClientManager();

        public override ZooKeeperClient DefaultClient
        {
            get
            {
                return GetCachedClient("lib_zookeeper");
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
