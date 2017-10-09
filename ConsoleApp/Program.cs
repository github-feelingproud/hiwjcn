using Elasticsearch.Net;
using Fleck;
using Lib.core;
using Lib.distributed;
using Lib.extension;
using Lib.helper;
using Lib.io;
using Nest;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            zk().Wait();
            //Task.Factory.StartNew(async () => await zk()).Wait();
            Console.WriteLine("finish");
            Console.ReadLine();
        }

        private static async Task zk()
        {
            try
            {
                //docker run --name some-zookeeper --restart always -p 2181:2181 -d zookeeper
                var client = new AlwaysOnZooKeeperClient("localhost:2181");
                client.OnRecconected += () =>
                {
                    Console.WriteLine("重新链接");
                };
                await Task.FromResult(1);
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void ws()
        {
            var server = new WebSocketServer("ws://0.0.0.0:8181");
            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    Console.WriteLine($"{socket.ConnectionInfo.Id}:Open");
                    var valid = false;
                    if (!valid)
                    {
                        socket.Send(new { close = true, reason = "验证未通过" }.ToJson());
                        socket.Close();
                    }
                };
                socket.OnClose = () => { Console.WriteLine("Close"); };
                socket.OnMessage = async msg => { await socket.Send(msg); };
            });
            Console.ReadLine();
            server.Dispose();
        }

        private static readonly string indexName = "productlist";
        private static readonly string ES_SERVERS = "http://localhost:9200";

        private static readonly ConnectionSettings setting = new ConnectionSettings(new SniffingConnectionPool(ES_SERVERS.Split('|', ',').Select(x => new Uri(x))));

        private static void PrepareES(Func<ElasticClient, bool> func)
        {
            var client = new ElasticClient(setting);
            func.Invoke(client);
        }

        public class SunData
        {
            [Key]
            public virtual int IID { get; set; }
            public virtual string UID { get; set; }
            public virtual string Data { get; set; }
            public virtual string ImagePath { get; set; }
            public virtual string Labels { get; set; }
        }

        interface p<T>
        { }
        interface human
        { }
        class basep
        { }
        class pp : p<string>, human
        { }

        /// <summary>
        /// 索引磁盘上的文件
        /// </summary>
        private static void IndexFiles()
        {
            PrepareES(client =>
            {
                var indexName = nameof(DiskFileIndex).ToLower();
                try
                {
                    client.CreateIndexIfNotExists(indexName, x => x.GetCreateIndexDescriptor<DiskFileIndex>());
                }
                catch (Exception e)
                {
                    e.AddErrorLog();
                }

                var txtFiles = new string[] {
                    ".txt", ".cs", ".css", ".js", ".cshtml", ".html"
                    , ".readme", ".json", ".config", ".md" };

                var maxSize = Com.MbToB(1);

                Com.FindFiles("D:\\XXXXXXXXXXXXXXXXXXXX\\", (f) =>
                {
                    Thread.Sleep(100);
                    try
                    {
                        if (!txtFiles.Any(x => ConvertHelper.GetString(f.Name).ToLower().EndsWith(x))
                        || ConvertHelper.GetString(f.FullName).ToLower().Contains("elasticsearch")
                        || f.Length > maxSize)
                        {
                            Console.WriteLine($"跳过文件：{f.FullName}");
                            return;
                        }
                        var model = new DiskFileIndex();
                        model.Path = f.FullName;
                        model.Name = f.Name;
                        model.Extension = f.Extension;
                        model.Size = f.Length;
                        model.Content = File.ReadAllText(model.Path);
                        var data = new List<DiskFileIndex>() { model };
                        client.AddToIndex(indexName, data.ToArray());
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }, (c) =>
                {
                    Console.WriteLine($"剩余任务：{c}");
                    if (c > 5000) { Thread.Sleep(400); }
                });
                Console.ReadLine();
                return true;
            });
        }

    }

    [ElasticsearchType(IdProperty = "Path", Name = "diskfileindex")]
    public class DiskFileIndex : IElasticSearchIndex
    {
        [String(Name = "Path", Index = FieldIndexOption.NotAnalyzed)]
        public virtual string Path { get; set; }

        [Number(Name = "Size", Index = NonStringIndexOption.NotAnalyzed)]
        public virtual long Size { get; set; }

        [String(Name = "Name", Index = FieldIndexOption.NotAnalyzed)]
        public virtual string Name { get; set; }

        [String(Name = "Extension", Index = FieldIndexOption.NotAnalyzed)]
        public virtual string Extension { get; set; }

        [String(Name = "Content", Analyzer = "ik_max_word", SearchAnalyzer = "ik_max_word")]
        public virtual string Content { get; set; }
    }

}