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

namespace Lib.distributed.zookeeper
{
    public class AlwaysOnZooKeeperClient : IDisposable
    {
        private bool IsDisposing = false;
        private ZooKeeperClient _client;

        private readonly string Host;

        private readonly ManualResetEvent _client_lock = new ManualResetEvent(false);
        private readonly object _create_client_lock = new object();

        public event Action Connected;
        public event Action UnConnected;
        public event Action OnRecconected;
        public event Action<Exception> OnError;

        public AlwaysOnZooKeeperClient(string host)
        {
            this.Host = host ?? throw new ArgumentNullException(nameof(host));
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
                        var reconnect_watcher = new ReconnectionWatcher(() =>
                        {
                            //服务可用
                            this._client_lock.Set();
                            this.Connected?.Invoke();
                        }, () =>
                        {
                            this._client_lock.Reset();
                            this.UnConnected?.Invoke();
                            //重新创建链接
                            this.ReConnect();
                        });
                        this._client = new ZooKeeperClient(this.Host, TimeSpan.FromSeconds(60), reconnect_watcher);
                    }
                }
            }
        }

        public virtual ZooKeeperClient GetClientManager()
        {
            try
            {
                this._client_lock.WaitOneOrThrow(TimeSpan.FromSeconds(30));

                if (this._client == null) { throw new Exception("zookeeper client is not prepared"); }

                return this._client;
            }
            catch (KeeperException.ConnectionLossException e)
            {
                this.OnError?.Invoke(e);
                //链接断开
                throw e;
            }
            catch (KeeperException.SessionExpiredException e)
            {
                this.OnError?.Invoke(e);
                //链接断开
                throw e;
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
                this._client?.Dispose();
            }
            catch (Exception e)
            {
                e.AddErrorLog();
            }
            this._client = null;
        }

        protected virtual void ReConnect()
        {
            //销毁的时候取消重试链接
            if (this.IsDisposing) { return; }

            this.CloseClient();
            this.CreateClient();
            this.OnRecconected.Invoke();
        }

        public virtual void Dispose()
        {
            this.IsDisposing = true;

            this.CloseClient();
            this._client_lock.Dispose();
        }
    }
}
