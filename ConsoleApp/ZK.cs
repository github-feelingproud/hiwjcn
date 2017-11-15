using Lib.distributed.zookeeper;
using Lib.extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
    public class ZK
    {
        public static async Task zk()
        {
            try
            {
                //docker run --name some-zookeeper --restart always -p 2181:2181 -d zookeeper
                var client = new AlwaysOnZooKeeperClient("zk.qipeilong.net:2181");
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
    }
}
