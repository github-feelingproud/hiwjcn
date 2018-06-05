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

                con.OnConnectedAsync += async () =>
                {
                    Console.WriteLine("链接成功" + DateTime.Now);
                    Console.WriteLine("============================");
                };
                con.OnError += (e) =>
                {
                    Console.WriteLine("链接异常" + e.Message + DateTime.Now);
                };
                con.OnSessionExpiredAsync += async () =>
                {
                    Console.WriteLine("session过期" + DateTime.Now);
                };
                con.OnRecconectingAsync += async () =>
                {
                    Console.WriteLine("重新链接" + DateTime.Now);
                };
                con.OnUnConnectedAsync += async () =>
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