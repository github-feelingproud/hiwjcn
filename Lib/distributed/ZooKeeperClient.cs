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

        public ZooKeeperClient(string configurationName = "zookeeper") : this(ZooKeeperClientSection.FromSection(configurationName)) { }

        public ZooKeeperClient(ZooKeeperClientSection configuration) : this(configuration.Server, configuration.SessionTimeOut) { }

        public ZooKeeperClient(string server, int sessionTimeOut)
        {
            if (!ValidateHelper.IsPlumpString(server))
            {
                throw new ArgumentNullException("zookeeper server can not be empty");
            }

            ConnectionLossTimeout = sessionTimeOut;

            _zookeeper = new ZooKeeper(server, TimeSpan.FromMilliseconds(sessionTimeOut), new ZooKeeperWatcher(this));
        }

        protected int ConnectionLossTimeout { get; set; }
        
        #region Invoke

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
        #endregion

        #region GetData
        /// <summary>获取数据</summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="path">路径</param>
        /// <returns>数据</returns>
        public T GetData<T>(string path) => GetData<T>(path, false);

        /// <summary>获取数据</summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="path">路径</param>
        /// <param name="watch">是否添加watch，默认false</param>
        /// <returns>数据</returns>
        public virtual T GetData<T>(string path, bool watch) =>
            Deserialize<T>(Invoke(path, (zookeeper, p) => zookeeper.GetData(p, watch, null)));

        #endregion

        #region SetData
        /// <summary>设置数据</summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="path">路径</param>
        /// <param name="objData">对象</param>
        /// <returns>是否成功</returns>
        public bool SetData<T>(string path, T objData) => SetData(path, objData, -1);

        /// <summary>设置数据</summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="path">路径</param>
        /// <param name="objData">对象</param>
        /// <param name="version">数据版本，默认-1</param>
        /// <returns>是否成功</returns>
        public virtual bool SetData<T>(string path, T objData, int version)
        {
            var buffer = Serialize(objData);
            return Invoke(path, (zookeeper, p) => zookeeper.SetData(p, buffer, version)) != null;
        }
        #endregion

        #region GetChildren
        public IEnumerable<string> GetChildren(string path) => GetChildren(path, false);

        public IEnumerable<string> GetChildren(string path, bool watch) => Invoke(path, (zookeeper, p) => zookeeper.GetChildren(p, watch));

        #endregion

        #region CreatePath
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
        #endregion

        #region Watch
        public Stat Watch(string path) => Invoke(path, (zookeeper, p) => zookeeper.Exists(p, true));

        public Stat Watch(string path, IWatcher watcher) => Invoke(path, (zookeeper, p) => zookeeper.Exists(p, watcher));

        #endregion

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
}
