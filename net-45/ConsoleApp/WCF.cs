using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Description;
using Lib.extension;
using Lib.rpc;

namespace ConsoleApp
{
    [ServiceContract]
    public interface xx
    {
        [OperationContract]
        DateTime Time();
    }

    [ServiceContractImpl]
    public class xxImpl : xx
    {
        public DateTime Time() => DateTime.Now;
    }

    [ServiceContract]
    public interface mm
    {
        [OperationContract]
        DateTime Date();
    }

    [ServiceContractImpl]
    public class mmImpl : mm
    {
        public DateTime Date() => DateTime.Now;
    }

    public static class WCF
    {
        public static void Serv()
        {
            try
            {
                ServiceHostManager.Host.StartService("http://localhost:10000/", typeof(WCF).Assembly);
                Console.WriteLine("服务已启动");
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.GetInnerExceptionAsJson());
            }
            finally
            {
                Lib.core.LibReleaseHelper.DisposeAll();
            }
        }
    }
}
