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
