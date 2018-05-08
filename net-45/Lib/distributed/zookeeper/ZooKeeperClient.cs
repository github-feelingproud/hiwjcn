﻿using Lib.core;
using Lib.distributed.zookeeper.watcher;
using Lib.extension;
using org.apache.zookeeper;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lib.distributed.zookeeper
{
    /// <summary>
    /// 资料：邮箱搜索“zookeeper资料”
    /// </summary>
    public class ZooKeeperClient : IDisposable
    {
        protected bool IsDisposing = false;
        private ZooKeeper _client;
        //param
        protected readonly string _host;
        protected readonly TimeSpan _timeout;
        protected readonly Watcher _connection_status_watcher;
        //lock
        private readonly ManualResetEvent _client_lock = new ManualResetEvent(false);
        private readonly object _create_client_lock = new object();
        //event
        public event Action OnConnected;
        public event Action OnUnConnected;
        public event Action OnSessionExpired;
        public event Action<Exception> OnError;

        public ZooKeeperClient(ZooKeeperConfigSection configuration) :
            this(configuration.Server, TimeSpan.FromMilliseconds(configuration.SessionTimeOut))
        {
            //
        }

        public ZooKeeperClient(string host, TimeSpan? timeout = null)
        {
            this._host = host ?? throw new ArgumentNullException(nameof(host));
            this._timeout = timeout ?? TimeSpan.FromSeconds(30);
            /*
            this._connection_status_watcher = new ReconnectionWatcher(() =>
            {
                //服务可用
                this._client_lock.Set();
                this.OnConnected?.Invoke();
            }, () =>
            {
                //服务不可用
                this._client_lock.Reset();
                this.OnUnConnected?.Invoke();
            });*/
            this._connection_status_watcher = new ConnectionStatusWatcher(status =>
            {
                this._client_lock.Reset();
                switch (status)
                {
                    case Watcher.Event.KeeperState.SyncConnected:
                    case Watcher.Event.KeeperState.ConnectedReadOnly:
                        //服务可用
                        this._client_lock.Set();
                        this.OnConnected?.Invoke();
                        break;

                    case Watcher.Event.KeeperState.Disconnected:
                        //链接丢失，等待再次连接
                        this.OnUnConnected?.Invoke();
                        break;

                    case Watcher.Event.KeeperState.Expired:
                        //session过期，重新创建客户端
                        this.OnSessionExpired?.Invoke();
                        break;

                    case Watcher.Event.KeeperState.AuthFailed:
                        //验证错误没必要再试尝试
                        this.Dispose();
                        var e = new Exception("zk auth验证失败，已经销毁所有链接");
                        e.AddErrorLog();
                        throw e;
                }
            });

            //connect
            this.CreateClient();
        }

        protected virtual void CreateClient()
        {
            if (this._client == null)
            {
                lock (this._create_client_lock)
                {
                    if (this._client == null)
                    {
                        this._client = new ZooKeeper(
                            this._host,
                            (int)this._timeout.TotalMilliseconds,
                            this._connection_status_watcher);
                    }
                }
            }
        }

        public ZooKeeper Client { get => this.GetClientManager(); }

        /// <summary>
        /// 等待可用链接，默认30秒超时
        /// </summary>
        /// <returns></returns>
        public virtual ZooKeeper GetClientManager(TimeSpan? timeout = null)
        {
            try
            {
                //等待连上
                this._client_lock.WaitOneOrThrow(timeout ?? TimeSpan.FromSeconds(30), "无法链接zk");

                if (this._client == null) { throw new Exception("zookeeper client is not prepared"); }

                return this._client;
            }
            catch (KeeperException.ConnectionLossException e)
            {
                this.OnError?.Invoke(e);
                //链接断开
                throw new Exception("zk链接丢失", e);
            }
            catch (KeeperException.SessionExpiredException e)
            {
                this.OnError?.Invoke(e);
                //链接断开
                throw new Exception("zk会话丢失", e);
            }
            catch (Exception e)
            {
                this.OnError?.Invoke(e);
                throw e;
            }
        }

        protected virtual void CloseClient()
        {
            try
            {
                if (this._client != null)
                {
                    Task.Factory.StartNew(async () => await this._client.closeAsync()).Wait();
                }
            }
            catch (Exception e)
            {
                this.OnError?.Invoke(e);
                e.AddErrorLog("关闭zk客户端失败");
            }
            finally
            {
                this._client = null;
            }
        }

        public virtual void Dispose()
        {
            this.IsDisposing = true;

            this.CloseClient();
            this._client_lock.Dispose();
        }
    }
}
