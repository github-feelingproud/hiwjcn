using Lib.distributed.zookeeper;
using Lib.extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.distributed.zookeeper.ServiceManager;
using Lib.helper;

namespace ConsoleApp
{
    [Lib.rpc.IsWcfContract(RelativePath = "jlk.svc")]
    public interface fsadf { }
    [Lib.rpc.IsWcfContract(RelativePath = "jflk.svc")]
    public interface fsfasfadf { }
    [Lib.rpc.IsWcfContract(RelativePath = "jlgk.svc")]
    public interface fsgsdadf { }
    [Lib.rpc.IsWcfContract(RelativePath = "jalk.svc")]
    public interface fsafasdfdf { }

    public class ZK
    {
        public static async Task zk()
        {
            try
            {
                //docker run --name some-zookeeper --restart always -p 2181:2181 -d zookeeper
                var client = new AlwaysOnZooKeeperClient("es.qipeilong.net:2181");
                client.OnRecconected += () =>
                {
                    Console.WriteLine("重新链接");
                };
                client.OnError += e =>
                {
                    Console.WriteLine(e.GetInnerExceptionAsJson());
                };

                await Task.FromResult(1);
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        
        public static async Task sub()
        {
            try
            {
                var host = "es.qipeilong.net:2181";
                //docker run --name some-zookeeper --restart always -p 2181:2181 -d zookeeper
                var client = new ServiceSubscribe(host);
                client.OnRecconected += () => { Console.WriteLine("重新链接"); };
                client.OnServiceChanged += () => { Console.WriteLine("服务发生更改"); };
                
                foreach (var i in Com.Range(1000))
                {
                    using (var reg = new ServiceRegister(host))
                    {
                        await reg.RegisterService("http://www.qpl.com/service/", "3", typeof(ZK).Assembly);
                        Console.WriteLine(client.AllService().ToJson());

                        await Task.Delay((int)TimeSpan.FromSeconds(20).TotalMilliseconds);
                    }
                }

                await Task.FromResult(1);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
