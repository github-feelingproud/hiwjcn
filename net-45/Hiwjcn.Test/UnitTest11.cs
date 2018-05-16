using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Reactive.Concurrency;
using System.Collections;
using Lib.helper;
using Lib.core;
using Lib.extension;
using Lib.io;
using System.Linq;
using Lib.task;
using Quartz;
using Lib.rpc;
using System.Threading;
using Lib.distributed.zookeeper.ServiceManager;
using System.Diagnostics;
using Lib.distributed.zookeeper;

namespace Hiwjcn.Test
{
    public interface IUser
    {
        Task<Lib.mvc.user.LoginUserInfo> GetLoginUserInfo(string uid, string name, int age);
    }

    [TestClass]
    public class UnitTest11
    {
        [TestMethod]
        public async Task fafafasdfhga()
        {
            await Task.Run(() =>
            {
                var con = new AlwaysOnZooKeeperClient("");
                con.OnConnected += () =>
                {
                    Debug.Write(nameof(con.OnConnected));
                };
                con.OnError += (e) =>
                {
                    Debug.Write(nameof(con.OnError));
                };
                con.OnRecconected += () =>
                {
                    Debug.Write(nameof(con.OnRecconected));
                };
                con.OnUnConnected += () =>
                {
                    Debug.Write(nameof(con.OnUnConnected));
                };
            });
            //
            var i = 0;
        }

        [TestMethod]
        public void fasdfhga()
        {
            var _lz = new Lazy_<ServiceSubscribe>(() => new ServiceSubscribe(""));

            foreach (var m in Com.Range(10))
            {
                try
                {
                    var service = _lz.Value.ResolveSvc<IUser>() ?? throw new Exception("服务下线");
                }
                catch (Exception e)
                {
                    //
                }
            }

            var _client_lock = new ManualResetEvent(false);
            _client_lock.WaitOne(TimeSpan.FromSeconds(5));
            _client_lock.WaitOne(TimeSpan.FromSeconds(5));
        }

        [TestMethod]
        public async Task fasdfasdhj()
        {
            try
            {
                var client = new WebApiClient<IUser>();
                await client.Instance.GetLoginUserInfo("uid-xxx", "name-wj", 3);
            }
            catch (Exception e)
            {
                //
            }
        }

        public interface order
        {
            Task<string> get_name(string uid);

            string get_count(int id);
        }

        [TestMethod]
        public void fasdfagsfdgsdfg()
        {
            var tp = typeof(order);
        }

        [TestMethod]
        public void TestMethod1()
        {
            Com.Range(100).ToObservable()
                .ObserveOn(Scheduler.ThreadPool)
                .ObserveOn(Scheduler.Default)
                .Subscribe(x =>
                {
                    Console.WriteLine(x);
                });
        }

        [TestMethod]
        public void lkjfsladhfasdkfhkj()
        {
            try
            {
                var a = typeof(Hiwjcn.Web.Controllers.AccountController).Assembly;
                var tps = a.GetTypes();
            }
            catch (Exception e)
            { }
        }

    }
}
