using Hiwjcn.Framework;
using Lib.extension;
using Lib.ioc;
using System;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            IocContext.Instance.AddExtraRegistrar(new CommonDependencyRegister());

            try
            {
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

            Console.WriteLine("finish");
            Console.ReadLine();
            Lib.core.LibReleaseHelper.DisposeAll();
        }
    }


}