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

    [ServiceContract]
    public interface mm
    {
        [OperationContract]
        DateTime Date();
    }

    public class mmImpl : mm
    {
        public DateTime Date() => DateTime.Now;
    }

    public static class WCF
    {
        public static void Serv()
        {
            var list = new List<ServiceHost>();

            try
            {
                {
                    var host = new ServiceHost(typeof(xxImpl), new Uri("http://localhost:10000/xx/"));
                    host.AddServiceEndpoint(typeof(xx), new BasicHttpBinding(), "xx");

                    var smb = new ServiceMetadataBehavior();
                    smb.HttpGetEnabled = true;
                    smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
                    host.Description.Behaviors.Add(smb);

                    host.Open();
                    list.Add(host);
                }

                {
                    var host = new ServiceHost(typeof(mmImpl), new Uri("http://localhost:10000/mm/"));
                    host.AddServiceEndpoint(typeof(mm), new BasicHttpBinding(), "mm");

                    var smb = new ServiceMetadataBehavior();
                    smb.HttpGetEnabled = true;
                    smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
                    host.Description.Behaviors.Add(smb);

                    host.Open();
                    list.Add(host);
                }
                Console.WriteLine("服务已启动");
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.GetInnerExceptionAsJson());
            }
            finally
            {
                foreach (var s in list)
                {
                    s.Close();
                    ((IDisposable)s).Dispose();
                }
            }
        }
    }
}
