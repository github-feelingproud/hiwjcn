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
            IocContext.Instance.AddExtraRegistrar(new CommonDependencyRegister());

            try
            {
                var con = new AlwaysOnZooKeeperClient("es.qipeilong.net:2181");
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