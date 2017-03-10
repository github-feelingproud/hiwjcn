using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lib.net;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using Lib.core;
using Lib.helper;
using HtmlParserSharp;
using HtmlParserSharp.Common;
using HtmlParserSharp.Core;
using Nest;
using Elasticsearch.Net;
using Dapper;
using Hiwjcn.Dal;
using System.ComponentModel.DataAnnotations;
using Fleck;
using Fleck.Handlers;
using Fleck.Helpers;
using System.Net;

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
                if (!client.IndexExists(indexName).Exists)
                {
                    var createIndexRes = client.CreateIndex(indexName,
                        x => x.Settings(s => s.NumberOfShards(5).NumberOfReplicas(1))
                        .Mappings(map => map.Map<DiskFileIndex>(m => m.AutoMap(5))));
                    if (!createIndexRes.IsValid)
                    {
                        Console.WriteLine("无法创建索引");
                        return false;
                    }
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
                        var bulk = new BulkRequest(indexName)
                        {
                            Operations = data.Select(x => new BulkIndexOperation<DiskFileIndex>(x)).ToArray()
                        };
                        var res = client.Bulk(bulk);
                        if (res.IsValid)
                        {
                            Console.WriteLine($"索引文件：{f.FullName}");
                        }
                        else
                        {
                            Console.WriteLine($"索引失败，文件：{f.FullName}");
                            Thread.Sleep(500);
                        }
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
                var queryresponse = client.Suggest<ProductListV2>(x => x.Phrase("phrase_suggest", m => m.Field(f => f.SeachTitle).Text("MAHLwjE")));
                if (queryresponse.IsValid)
                {
                }
                else
                {
                    Console.WriteLine("查询失败");
                }
                Console.ReadLine();
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

    [ElasticsearchType(IdProperty = "UKey", Name = "ProductList")]
    public class ProductListV2
    {
        [String(Name = "UKey", Index = FieldIndexOption.NotAnalyzed)]
        public string UKey { get; set; }

        [String(Name = "ProductId", Index = FieldIndexOption.NotAnalyzed)]
        public string ProductId { get; set; }

        [String(Name = "TraderId", Index = FieldIndexOption.NotAnalyzed)]
        public string TraderId { get; set; }

        [String(Name = "PlatformCatalogId", Index = FieldIndexOption.NotAnalyzed)]
        public string PlatformCatalogId { get; set; }

        [String(Name = "BrandId", Index = FieldIndexOption.NotAnalyzed)]
        public string BrandId { get; set; }

        [Number(Name = "PAvailability", Index = NonStringIndexOption.NotAnalyzed)]
        public int PAvailability { get; set; }

        [Number(Name = "PIsRemove", Index = NonStringIndexOption.NotAnalyzed)]
        public int PIsRemove { get; set; }

        [Number(Name = "UpAvailability", Index = NonStringIndexOption.NotAnalyzed)]
        public int UpAvailability { get; set; }

        [Number(Name = "UpIsRemove", Index = NonStringIndexOption.NotAnalyzed)]
        public int UpIsRemove { get; set; }

        [String(Name = "UserSku", Index = FieldIndexOption.NotAnalyzed)]
        public string UserSku { get; set; }

        [Number(Name = "IsGroup", Index = NonStringIndexOption.NotAnalyzed)]
        public int IsGroup { get; set; }

        [Number(Name = "UpiId", Index = NonStringIndexOption.NotAnalyzed)]
        public int UpiId { get; set; }

        /// <summary>
        /// 销量
        /// </summary>
        [Number(Name = "SalesVolume", Index = NonStringIndexOption.NotAnalyzed)]
        public int SalesVolume { get; set; }

        /// <summary>
        /// 是否有货
        /// </summary>
        [Number(Name = "InventoryStatus", Index = NonStringIndexOption.NotAnalyzed)]
        public int InventoryStatus { get; set; }


        [Number(Name = "SalesPrice", Index = NonStringIndexOption.NotAnalyzed)]
        public decimal SalesPrice { get; set; }

        [String(Name = "ShopName", Analyzer = "ik_max_word", SearchAnalyzer = "ik_max_word")]
        public string ShopName { get; set; }

        [String(Name = "ShopNamePinyin", Analyzer = "pinyin_analyzer", SearchAnalyzer = "pinyin_analyzer")]
        public string ShopNamePinyin { get; set; }

        [String(Name = "SeachTitle", Analyzer = "ik_max_word", SearchAnalyzer = "ik_max_word")]
        public string SeachTitle { get; set; }

        [Date(Name = "UpdatedDate")]
        public DateTime UpdatedDate { get; set; }

        [String(Name = "ShowCatalogIdList", Index = FieldIndexOption.NotAnalyzed)]
        public string[] ShowCatalogIdList { get; set; }

        [String(Name = "PlatformCatalogIdList", Index = FieldIndexOption.NotAnalyzed)]
        public string[] PlatformCatalogIdList { get; set; }

        [String(Name = "ProductAttributes", Index = FieldIndexOption.NotAnalyzed)]
        public string[] ProductAttributes { get; set; }
    }

}