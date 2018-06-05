using Lib.distributed.zookeeper;
using Lib.extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.distributed.zookeeper.ServiceManager;
using Lib.helper;
using WangJunGatrWay.Service;
using System.ServiceModel;

namespace WangJunGatrWay.Service
{
    [ServiceContract]
    public interface IService1
    {
        [OperationContract]
        string GetData(int value);
    }
}

namespace ConsoleApp
{
    public interface fsadf { }
    public interface fsfasfadf { }
    public interface fsgsdadf { }
    public interface fsafasdfdf { }

    public class ZK
    {
        public class Caller : Lib.rpc.ServiceClient<IService1>
        {
            public Caller(ServiceSubscribe client) : base(client.ResolveSvc<IService1>())
            { }
        }

        public static async Task call()
        {
            ServiceSubscribe client = null;
            try
            {
                var host = "**";
                //docker run --name some-zookeeper --restart always -p 2181:2181 -d zookeeper
                client = new ServiceSubscribe(host);
                client.OnConnectedAsync += async () => { Console.WriteLine("重新链接"); };
                client.OnServiceChangedAsync += async () =>
                {
                    Console.WriteLine("服务发生更改");
                    var s = client.AllService();
                    if (ValidateHelper.IsPlumpList(s))
                    {
                        s.Select(x => $"{x.FullPathName}===={x.Url}").ToList().ForEach(Console.WriteLine);
                    }
                    else
                    {
                        Console.WriteLine("没有服务");
                    }
                };

                foreach (var i in Com.Range(10000000))
                {
                    while (client.AllService().Count <= 0)
                    {
                        Console.WriteLine($"等待服务上线==={i}");
                        await Task.Delay(TimeSpan.FromSeconds(1));
                    }

                    using (var c = new Caller(client))
                    {
                        try
                        {
                            var data = c.Instance.GetData(i);
                            Console.WriteLine($"服务返回数据：{data}");
                            await Task.Delay(TimeSpan.FromSeconds(1));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                }

                await Task.FromResult(1);
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                client?.Dispose();
            }
        }
    }
}
