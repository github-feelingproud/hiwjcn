using Elasticsearch.Net;
using Fleck;
using Lib.core;
using Lib.extension;
using Lib.helper;
using Nest;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new WebSocketServer("ws://0.0.0.0:8181");
            server.Start(socket =>
            {
                socket.OnOpen = () => { Console.WriteLine("Open"); };
                socket.OnClose = () => { Console.WriteLine("Close"); };
                socket.OnMessage = async msg => { await socket.Send(msg); };
            });
            Console.ReadLine();
        }

        private static readonly string indexName = "productlist";
        private static readonly string ES_SERVERS = "http://localhost:9200";

        private static readonly ConnectionSettings setting = new ConnectionSettings(new SniffingConnectionPool(ES_SERVERS.Split('|', ',').Select(x => new Uri(x))));

        private static void PrepareES(Func<ElasticClient, bool> func)
        {
            var client = new ElasticClient(setting);
            func.Invoke(client);
        }

        private static List<ProductListV2> LoadData(int page)
        {
            var client = new ElasticClient(new Uri("http://172.16.42.28:9200/"));
            var sd = new SearchDescriptor<ProductListV2>();
            sd = sd.Index("productlist");
            int pagesize = 100;
            var range = PagerHelper.GetQueryRange(page, pagesize);
            sd = sd.Skip(range[0]).Take(range[1]);
            var response = client.Search<ProductListV2>(x => sd);
            if (response.IsValid)
            {
                return response?.Hits.Select(x => x.Source).Where(x => x != null).ToList();
            }
            return null;
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
                    client.CreateIndexIfNotExists(indexName, x => x.DeaultCreateIndexDescriptor<DiskFileIndex>());
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
                        client.AddToIndex(indexName, data);
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

        private static void Index()
        {
            PrepareES(client =>
            {
                if (!client.IndexExists(indexName).Exists)
                {
                    var createIndexRes = client.CreateIndex(indexName,
                        x => x.Settings(s => s.NumberOfShards(5).NumberOfReplicas(1))
                        .Mappings(map => map.Map<ProductListV2>(m => m.AutoMap(5))));
                    if (!createIndexRes.IsValid || createIndexRes.OriginalException != null)
                    {
                        Console.WriteLine("无法创建索引");
                        return false;
                    }
                }
                int count = 0;
                for (int i = 0; i < 10000; ++i)
                {
                    if (i > 99)
                    {
                        //
                    }

                    var data = LoadData(i + 1);
                    if (!ValidateHelper.IsPlumpList(data)) { break; }
                    data.ForEach(x => { x.ShopNamePinyin = x.ShopName; });
                    var bulk = new BulkRequest(indexName)
                    {
                        Operations = data.Select(x => new BulkIndexOperation<ProductListV2>(x)).ToArray()
                    };
                    var res = client.Bulk(bulk);
                    if (res.IsValid)
                    {
                        Console.WriteLine($"第{i + 1}页，创建{data.Count}个索引");
                        count += data.Count;
                    }
                    else
                    {
                        if (res.OriginalException != null)
                        {
                            Console.WriteLine(res.OriginalException.Message);
                        }
                        else
                        {
                            Console.WriteLine("无法创建索引");
                        }
                        Thread.Sleep(1000 * 5);
                    }
                }
                Console.WriteLine($"创建或者修改了{count}条索引");
                Console.ReadLine();
                return true;
            });
        }

        private static void Query()
        {
            PrepareES(client =>
            {
                var temp = new ProductListV2();
                var sd = new SearchDescriptor<ProductListV2>();
                var query = new QueryContainer();
                query = query && new MatchQuery() { Field = nameof(temp.SeachTitle), Query = "机油" };
                sd = sd.Query(x => query);
                sd = sd.Skip(0).Take(100);

                sd = sd.Highlight(x => x.PreTags("<span class='hit'>").PostTags("</span>")
                .Fields(f => f.Field(fd => fd.ShopName)));

                var queryresponse = client.Search<ProductListV2>(x => sd);
                if (queryresponse.IsValid)
                {
                    var list = queryresponse?.Hits.Select(x => x.Source).Where(x => x != null).ToList();
                    if (ValidateHelper.IsPlumpList(list))
                    {
                        foreach (var model in list)
                        {
                            Console.WriteLine(model.SeachTitle);
                        }
                    }
                    else
                    {
                        Console.WriteLine("没有找到数据");
                    }
                }
                else
                {
                    Console.WriteLine("查询失败");
                }
                Console.ReadLine();
                return true;
            });
        }

        private static void Suggest()
        {
            PrepareES(client =>
            {
                try
                {
                    var data = client.SuggestKeyword<ProductListV2>(x => x.SeachTitle, "MAHLwjE");
                }
                catch
                { }
                return true;
            });
        }
    }


    [ElasticsearchType(IdProperty = "Path", Name = "diskfileindex")]
    public class DiskFileIndex
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