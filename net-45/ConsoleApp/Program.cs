using Hiwjcn.Framework;
using Lib.distributed.zookeeper;
using Lib.extension;
using Lib.ioc;
using System;
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
                var con = new AlwaysOnZooKeeperClient("es.qipeilong.net:2181");
                con.OnConnected += () =>
                {
                    Console.WriteLine(nameof(con.OnConnected));
                };
                con.OnError += (e) =>
                {
                    Console.WriteLine(nameof(con.OnError));
                };
                con.OnRecconected += () =>
                {
                    Console.WriteLine(nameof(con.OnRecconected));
                };
                con.OnUnConnected += () =>
                {
                    Console.WriteLine(nameof(con.OnUnConnected));
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