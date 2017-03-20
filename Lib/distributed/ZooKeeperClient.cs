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

namespace Lib.distributed
{
    /// <summary>
    /// 
    /// 资料：邮箱搜索“zookeeper资料”
    /// </summary>
    public class ZooKeeperClient : SerializeBase, IDisposable
    {
        private readonly IZooKeeper _zookeeper;

        private ManualResetEvent _resetEvent = new ManualResetEvent(false);

        public ZooKeeperClient(string configurationName = "zookeeper") : this(ZooKeeperConfigSection.FromSection(configurationName)) { }

        public ZooKeeperClient(ZooKeeperConfigSection configuration)
        {
            if (!ValidateHelper.IsPlumpString(configuration.Server))
            {
                throw new ArgumentNullException("zookeeper server can not be empty");
            }

            ConnectionLossTimeout = configuration.SessionTimeOut;

            _zookeeper = new ZooKeeper(
                configuration.Server,
                TimeSpan.FromMilliseconds(configuration.SessionTimeOut),
                new ZooKeeperWatcher(this));
        }

        protected int ConnectionLossTimeout { get; set; }

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

        public T GetData<T>(string path) => GetData<T>(path, false);

        public virtual T GetData<T>(string path, bool watch) =>
            Deserialize<T>(Invoke(path, (zookeeper, p) => zookeeper.GetData(p, watch, null)));

        public bool SetData<T>(string path, T objData) => SetData(path, objData, -1);

        public virtual bool SetData<T>(string path, T objData, int version)
        {
            var buffer = Serialize(objData);
            return Invoke(path, (zookeeper, p) => zookeeper.SetData(p, buffer, version)) != null;
        }

        public IEnumerable<string> GetChildren(string path) => GetChildren(path, false);

        public IEnumerable<string> GetChildren(string path, bool watch) => Invoke(path, (zookeeper, p) => zookeeper.GetChildren(p, watch));

        public bool CreatePersistentPath(string path)
        {
            if (Invoke(path, (zookeeper, p) => zookeeper.Exists(p, false)) != null)
                return false;

            var prePath = "";
            foreach (var item in path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries))
            {
                prePath = prePath + "/" + item;
                if (Invoke(prePath, (zookeeper, p) => zookeeper.Exists(p, false)) != null)
                {
                    continue;
                }

                try
                {
                    Invoke(prePath, (zookeeper, p) => zookeeper.Create(p, null, Ids.OPEN_ACL_UNSAFE, CreateMode.Persistent));
                }
                catch (KeeperException.NodeExistsException e)
                {
                    e.AddErrorLog();
                }
            }

            return true;
        }
        public string CreateSequential(string path, bool persistent) => CreateSequential(path, null, persistent);

        public string CreateSequential(string path, byte[] data, bool persistent) =>
            Invoke(path, (zookeeper, p) =>
                zookeeper.Create(p,
                    data,
                    Ids.OPEN_ACL_UNSAFE, persistent ? CreateMode.PersistentSequential : CreateMode.EphemeralSequential))
                .Substring(path.Length);

        public void DeleteNode(string path) => Invoke(path, (zookeeper, p) => zookeeper.Delete(p, -1));

        public Stat Watch(string path) => Invoke(path, (zookeeper, p) => zookeeper.Exists(p, true));

        public Stat Watch(string path, IWatcher watcher) => Invoke(path, (zookeeper, p) => zookeeper.Exists(p, watcher));

        public Action<ZooKeeperClient> OnDisconnected;
        public Action<ZooKeeperClient> OnReconnected;
        public Action<ZooKeeperClient> OnSessionExpired;
        public Action<ZooKeeperClient> OnDataChanged;
        public Action<ZooKeeperClient> OnNodeDeleted;
        public Action<ZooKeeperClient> OnNodeCreated;
        public Action<ZooKeeperClient> OnNodeChildrenChanged;
        public Action<ZooKeeperClient> OnClosed;

        #region Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            _disposed = true;

            if (disposing)
            {
                //释放托管资源，比如将对象设置为null
            }

            //释放非托管资源
            _zookeeper.Dispose();

            var resetEvent = _resetEvent;
            _resetEvent = null;

            resetEvent.Dispose();
        }

        ~ZooKeeperClient()
        {
            Dispose(false);
        }
        #endregion

        class ZooKeeperWatcher : IWatcher
        {
            private readonly ZooKeeperClient _zookeeper;
            public ZooKeeperWatcher(ZooKeeperClient zookeeper)
            {
                _zookeeper = zookeeper;
            }

            private bool _disconnected;

            public void Process(WatchedEvent @event)
            {
                $"ZooKeeperWatcher: State={@event.State}, EventType={@event.Type}, Path={@event.Path}".AddBusinessInfoLog();

                switch (@event.State)
                {
                    case KeeperState.Disconnected:
                        if (_zookeeper._disposed)
                        {
                            _zookeeper.OnClosed?.Invoke(_zookeeper);
                        }
                        else
                        {
                            _disconnected = true;
                            _zookeeper._resetEvent?.Reset();
                            _zookeeper.OnDisconnected?.Invoke(_zookeeper);
                        }
                        break;
                    case KeeperState.SyncConnected:
                        _zookeeper._resetEvent?.Set();
                        if (_disconnected)
                        {
                            _disconnected = false;
                            _zookeeper.OnReconnected?.Invoke(_zookeeper);
                        }
                        break;
                    case KeeperState.Expired:
                        _zookeeper.OnSessionExpired?.Invoke(_zookeeper);
                        break;
                    case KeeperState.NoSyncConnected:
                    default:
                        break;
                }

                switch (@event.Type)
                {
                    case EventType.NodeCreated:
                        _zookeeper.OnNodeCreated?.Invoke(_zookeeper);
                        break;
                    case EventType.NodeDeleted:
                        _zookeeper.OnNodeDeleted?.Invoke(_zookeeper);
                        break;
                    case EventType.NodeDataChanged:
                        _zookeeper.OnDataChanged?.Invoke(_zookeeper);
                        break;
                    case EventType.NodeChildrenChanged:
                        _zookeeper.OnNodeChildrenChanged?.Invoke(_zookeeper);
                        break;
                    case EventType.None:
                    default:
                        break;
                }
            }
        }

    }

    /// <summary>
    /// 链接管理
    /// </summary>
    public static class ZooKeeperClientManager
    { }

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
