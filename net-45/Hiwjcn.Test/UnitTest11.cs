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
    [TestClass]
    public class UnitTest11
    {
        [TestMethod]
        public async Task fasdfhga()
        {
            var data = new int[] { 1, 2 };
            var ran = new Random((int)DateTime.Now.Ticks);

            var all_data = Com.Range(20).Select(d => Task.Run(() => Com.Range(10000)
                   .Select(x => ran.Choice(data))
                   .GroupBy(x => x)
                   .Select(x => new { item = x.Key, count = x.Count() })
                   .ToList()));

            var res_data = await Task.WhenAll(all_data);
            

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
