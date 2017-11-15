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

namespace Lib.distributed.zookeeper.ServiceManager
{
    public class ServiceSubscribe
    {
        public class AlwaysOnZooKeeperClient : Watcher, IDisposable
        {
            private readonly string Host;
            private readonly string ServicePath;
            private bool IsClosing = false;

            private ZooKeeperClient _client;

            private readonly ManualResetEvent _client_lock = new ManualResetEvent(false);
            private readonly object _create_client_lock = new object();
            private readonly object _reconnection_lock = new object();
            private readonly AutoResetEvent _fetch_data_lock = new AutoResetEvent(true);

            public event Action OnRecconected;
            public event Action OnFetchingData;
            public event Action<Exception> OnError;

            private readonly Dictionary<string, string> ServiceData = new Dictionary<string, string>();

            private readonly SerializeHelper _serialize = new SerializeHelper();

            public AlwaysOnZooKeeperClient(string host) : this(host, "/QPL/WCF")
            { }

            public AlwaysOnZooKeeperClient(string host, string service_path)
            {
                this.Host = host ?? throw new ArgumentNullException(nameof(host));
                this.ServicePath = service_path ?? throw new ArgumentNullException(nameof(service_path));
                if (!this.ServicePath.StartsWith("/") || this.ServicePath.EndsWith("/"))
                {
                    throw new Exception($"{nameof(service_path)}必须以/开头，并且不能以/结尾");
                }

                this.Init();
            }

            private void Init()
            {
                this.CreateNewClient();
                Task.Factory.StartNew(async () =>
                {
                    await this.InitPath();
                    await this.FetchChildrenDataAndWatch();
                }).Wait();
            }

            private async Task InitPath()
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
                lock (this._create_client_lock)
                {
                    if (this._client != null)
                    {
                        this.CloseClient();
                    }

                    if (this._client == null)
                    {
                        this.IsClosing = false;
                        var reconnect_watcher = new ReconnectionWatcher(() =>
                        {
                            this._client_lock.Set();
                        }, () =>
                        {
                            this._client_lock.Reset();
                            //重新创建链接
                            this.CreateNewClient();
                        });
                        this._client = new ZooKeeperClient(this.Host, TimeSpan.FromSeconds(60), reconnect_watcher);
                    }
                }
            }

            private async Task UseClient(Func<ZooKeeperClient, Task> action)
            {
                try
                {
                    this._client_lock.WaitOneOrThrow(TimeSpan.FromSeconds(30));

                    if (this._client == null) { throw new Exception("zookeeper client is not prepared"); }


                    var start = DateTime.Now;
                    while (!(this._client?.Client?.getState() == ZooKeeper.States.CONNECTED))
                    {
                        if ((DateTime.Now - start).TotalSeconds > 10)
                        {
                            throw new Exception("等待可用链接超时");
                        }
                        await Task.Delay(5);
                    }

                    await action.Invoke(this._client);
                }
                catch (KeeperException.ConnectionLossException e)
                {
                    //链接断开
                    throw e;
                }
                catch (KeeperException.SessionExpiredException e)
                {
                    //链接断开
                    throw e;
                }
            }

            public async Task RegisterService<T>(string base_url) => await this.RegisterService(typeof(T), base_url);

            public async Task RegisterService(Type contract, string base_url)
            {
                var node_name = $"{contract.Assembly.FullName}-{contract.FullName}".Replace(' '.ToString(), string.Empty);
                var attr = contract.GetCustomAttributes_<IsWcfContractAttribute>().FirstOrDefault() ??
                    throw new Exception($"{contract.FullName} has no attribute:{nameof(IsWcfContractAttribute)}");
                var svc = $"{base_url}/{attr.RelativePath}";
                await this.UseClient(async client =>
                {
                    var path = $"{this.ServicePath}/{node_name}";
                    var data = this._serialize.Serialize(svc);
                    if (await client.Client.ExistAsync_(path))
                    {
                        await client.Client.setDataAsync(path, data);
                    }
                    else
                    {
                        await client.Client.CreatePersistentPathIfNotExist(path, data);
                    }
                });
            }

            private async Task FetchChildrenDataAndWatch()
            {
                await this.UseClient(async client =>
                {
                    if (await client.Client.ExistAsync_(this.ServicePath))
                    {
                        var child = await client.Client.getChildrenAsync(this.ServicePath, this);
                        Console.WriteLine($"找到节点{",".Join_(child.Children)}");
                        /*
                        var dict = new Dictionary<string, string>();
                        foreach (var x in child.Children)
                        {
                            try
                            {
                                var p = $"{this.ServicePath}/{x}";
                                var data = await client.Client.getDataAsync(p);
                                var svc = this._serialize.Deserialize<string>(data.Data);
                                dict[x] = svc;
                            }
                            catch (Exception e)
                            {
                                e.AddErrorLog();
                            }
                        }
                        this.ServiceData.Clear();
                        this.ServiceData.AddDict(dict);*/
                    }
                    else
                    {
                        throw new Exception($"不存在节点:{this.ServicePath}");
                    }
                    this.OnFetchingData.Invoke();
                });
            }

            private readonly HttpClient _httpclient = HttpClientManager.Instance.DefaultClient;

            public void HeartBeat()
            {
                Task.Factory.StartNew(async () =>
                {
                    await this.FetchChildrenDataAndWatch();
                    foreach (var kv in this.ServiceData)
                    {
                        try
                        {
                            using (var response = await this._httpclient.GetAsync(kv.Value))
                            {
                                if (!response.IsSuccessStatusCode)
                                {
                                    throw new Exception($"{kv.Value}无法请求");
                                }
                            }
                        }
                        catch
                        {
                            await this.UseClient(async client =>
                            await client.Client.DeleteNodeRecursively_($"{this.ServicePath}/{kv.Key}"));
                        }
                    }
                }).Wait();
            }

            private void CloseClient()
            {
                this.IsClosing = true;
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

            private void ReConnect()
            {
                lock (this._reconnection_lock)
                {
                    var real_time_state = this._client?.Client?.getState();
                    var connecting_or_connected = new ZooKeeper.States?[] { ZooKeeper.States.CONNECTED, ZooKeeper.States.CONNECTING };
                    if (connecting_or_connected.Contains(real_time_state))
                    {
                        //已经链接，或者正在链接
                        return;
                    }
                    this.CloseClient();

                    this.Init();
                    this.OnRecconected.Invoke();
                }
            }

            private async Task NodeChildrenChanged(string path)
            {
                if (path == this.ServicePath)
                {
                    this._fetch_data_lock.WaitOneOrThrow(TimeSpan.FromSeconds(5));
                    try
                    {
                        //服务发生改变
                        await this.FetchChildrenDataAndWatch();
                    }
                    finally
                    {
                        this._fetch_data_lock.Set();
                    }

                    //if (!Monitor.TryEnter(this._fetch_data_lock, TimeSpan.FromSeconds(5)))
                    //{
                    //    throw new Exception("抢锁失败");
                    //}
                    //try
                    //{
                    //    //服务发生改变
                    //    await this.FetchChildrenDataAndWatch();
                    //}
                    //finally
                    //{
                    //    Monitor.Exit(this._fetch_data_lock);
                    //}
                }
            }

            public override async Task process(WatchedEvent @event)
            {
                try
                {
                    if (this.IsClosing) { return; }

                    var event_type = @event.get_Type();
                    var zk_status = @event.getState();
                    var path = @event.getPath();

                    if (zk_status == Event.KeeperState.SyncConnected)
                    {
                        Console.WriteLine("链接可用");
                        this._client_lock.Set();
                    }
                    else
                    {
                        Console.WriteLine("链接不可用");
                        this._client_lock.Reset();
                    }

                    switch (zk_status)
                    {
                        case Event.KeeperState.AuthFailed:
                            Console.WriteLine(nameof(Event.KeeperState.AuthFailed));
                            break;
                        case Event.KeeperState.ConnectedReadOnly:
                            Console.WriteLine(nameof(Event.KeeperState.ConnectedReadOnly));
                            break;
                        case Event.KeeperState.Disconnected:
                            Console.WriteLine(nameof(Event.KeeperState.Disconnected));
                            this.ReConnect();
                            break;
                        case Event.KeeperState.Expired:
                            Console.WriteLine(nameof(Event.KeeperState.Expired));
                            this.ReConnect();
                            break;
                        case Event.KeeperState.SyncConnected:
                            break;
                        default:
                            break;
                    }

                    switch (event_type)
                    {
                        case Event.EventType.NodeChildrenChanged:
                            //注册节点发生改变
                            await this.NodeChildrenChanged(path);
                            break;
                        case Event.EventType.NodeCreated:
                            break;
                        case Event.EventType.NodeDataChanged:
                            break;
                        case Event.EventType.NodeDeleted:
                            break;
                        case Event.EventType.None:
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception e)
                {
                    this.OnError.Invoke(e);
                    e.AddErrorLog();
                }

                await Task.FromResult(1);
            }

            public void Dispose()
            {
                this.CloseClient();
                this._client_lock.Dispose();
            }
        }
    }
}
