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

namespace Hiwjcn.Test
{
    [TestClass]
    public class UnitTest11
    {
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
