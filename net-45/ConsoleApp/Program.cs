using Hiwjcn.Framework;
using Lib.distributed.zookeeper;
using Lib.distributed.zookeeper.ServiceManager;
using Lib.extension;
using Lib.ioc;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            AutofacIocContext.Instance.AddExtraRegistrar(new CommonDependencyRegister());

            try
            {
                var con = new ServiceRegister("***", () => new List<ContractModel>() { });

                con.OnConnected += () =>
                {
                    Console.WriteLine("链接成功" + DateTime.Now);
                    Console.WriteLine("============================");
                };
                con.OnError += (e) =>
                {
                    Console.WriteLine("链接异常" + e.Message + DateTime.Now);
                };
                con.OnSessionExpired += () =>
                {
                    Console.WriteLine("session过期" + DateTime.Now);
                };
                con.OnRecconecting += () =>
                {
                    Console.WriteLine("重新链接" + DateTime.Now);
                };
                con.OnUnConnected += () =>
                {
                    Console.WriteLine("链接断开" + DateTime.Now);
                };

                //ES.IndexFiles();

                //FleckWS.ws();

                //MQ.Consumer();

                //ZK.call().Wait();

                //WCF.Serv();

                //JobManager.Start();
            }
            catch (Exception e)
            {
                //
                Console.WriteLine(e.GetInnerExceptionAsJson());
            }

            Console.ReadLine();
            Console.WriteLine("finish");
            Console.ReadLine();
            Lib.core.LibReleaseHelper.DisposeAll();
        }
    }


}