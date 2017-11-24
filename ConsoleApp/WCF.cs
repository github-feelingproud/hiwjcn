using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Description;
using Lib.extension;

namespace ConsoleApp
{
    [ServiceContract]
    public interface xx
    {
        [OperationContract]
        DateTime Time();
    }

    public class xxImpl : xx
    {
        public DateTime Time() => DateTime.Now;
    }

    public static class WCF
    {
        public static void Serv()
        {
            using (var host = new ServiceHost(typeof(xxImpl), new Uri("http://localhost:10000/")))
            {
                try
                {
                    host.AddServiceEndpoint(typeof(xx), new BasicHttpBinding(), "hello");

                    var smb = new ServiceMetadataBehavior();
                    smb.HttpGetEnabled = true;
                    smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
                    host.Description.Behaviors.Add(smb);

                    host.Open();
                    Console.WriteLine("服务已启动");
                    Console.ReadKey();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.GetInnerExceptionAsJson());
                }
                finally
                {
                    host.Close();
                }
            }
        }
    }
}
