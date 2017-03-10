using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Nest;
using System.Net;
using System.Configuration;
using Lib.helper;

namespace Lib.extension
{
    public static class ElasticsearchExtension
    {
        /// <summary>
        /// 默认的shards和replicas
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static CreateIndexDescriptor DeaultCreateIndexDescriptor<T>(this CreateIndexDescriptor x) where T : class
        {
            return x.Settings(s => s.NumberOfShards(5).NumberOfReplicas(1)).Mappings(map => map.Map<T>(m => m.AutoMap(5)));
        }

        /// <summary>
        /// 如果索引不存在就创建
        /// </summary>
        /// <param name="client"></param>
        /// <param name="indexName"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static bool CreateIndexIfNotExists(this IElasticClient client, string indexName,
            Func<CreateIndexDescriptor, ICreateIndexRequest> selector = null)
        {
            indexName = indexName.ToLower();

            if (client.IndexExists(indexName).Exists)
                return true;

            var response = client.CreateIndex(indexName, selector);
            if (response.IsValid)
                return true;

            response.LogError();

            return false;
        }

        /// <summary>
        /// 添加到索引
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="indexName"></param>
        /// <param name="data"></param>
        public static void AddToIndex<T>(this ElasticClient client, string indexName, IEnumerable<T> data) where T : class
        {
            var bulk = new BulkRequest(indexName)
            {
                Operations = data.Select(x => new BulkIndexOperation<T>(x)).ToArray()
            };
            var response = client.Bulk(bulk);
            if (!response.IsValid)
            {
                response.LogError();
                throw new Exception("创建索引错误", response.OriginalException);
            }
        }

        /// <summary>
        /// 给关键词添加高亮
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sd"></param>
        /// <param name="pre"></param>
        /// <param name="after"></param>
        public static void AddHighlightWrapper<T>(this SearchDescriptor<T> sd, string pre = "<em>", string after = "</em>") where T : class
        {
            sd.Highlight(x => x.PreTags(pre).PostTags(after));
        }

        /// <summary>
        /// 获取聚合
        /// 升级到5.0，这个方法不可用，需要改动
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="response"></param>
        /// <returns></returns>
        private static Dictionary<string, List<KeyedBucket>> GetAggs<T>(this ISearchResponse<T> response) where T : class, new()
        {
            return response?.Aggregations?.ToDictionary(
                    x => x.Key,
                    x => (x.Value as BucketAggregate)?.Items.Select(i => (i as KeyedBucket)).Where(i => i?.DocCount > 0).ToList()
                    )?.Where(x => ValidateHelper.IsPlumpList(x.Value)).ToDictionary(x => x.Key, x => x.Value);
        }

        /// <summary>
        /// 记录返回错误
        /// </summary>
        /// <param name="response"></param>
        public static void LogError(this IResponse response)
        {
            if (response.ServerError?.Error == null)
            {
                if (response.OriginalException != null)
                    response.OriginalException.AddErrorLog();

                return;
            }
        }

        /// <summary>
        /// 开启链接调试
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="handler"></param>
        public static void EnableDebug(this ConnectionSettings setting, Action<IApiCallDetails> handler)
        {
            if (handler == null)
            {
                handler = x =>
                {
                    if (x.OriginalException != null)
                    {
                        x.OriginalException.AddErrorLog();
                    }
                    new
                    {
                        debuginfo = x.DebugInformation,
                        url = x.Uri.ToString(),
                        success = x.Success,
                        method = x.HttpMethod.ToString(),
                        DeprecationWarnings = x.DeprecationWarnings
                    }.ToJson().AddBusinessInfoLog();
                };
            }
            setting.DisableDirectStreaming();
            setting.OnRequestCompleted(handler);
        }
    }

    /// <summary>
    /// es
    /// https://www.elastic.co/products/elasticsearch
    /// </summary>
    public static class ElasticsearchHelper
    {
        //创建连接池Elasticsearch
        private static readonly ConnectionSettings ConnectionSettings;

        static ElasticsearchHelper()
        {
            var constr = ConfigurationManager.ConnectionStrings["ES"]?.ConnectionString;
            if (!ValidateHelper.IsPlumpString(constr))
            {
                return;
            }
            var urls = constr.Split('|', ';', ',').Select(s => new Uri(s));
            ConnectionSettings = new ConnectionSettings(new StaticConnectionPool(urls));

            ConnectionSettings.MaximumRetries(2);
        }

        /// <summary>
        /// 获取连接池
        /// </summary>
        /// <returns></returns>
        public static ConnectionSettings GetConnectionSettings() => ConnectionSettings;

        /// <summary>
        /// 获取链接对象
        /// </summary>
        /// <returns></returns>
        public static IElasticClient CreateClient() => new ElasticClient(ConnectionSettings);

        /// <summary>
        /// 关闭连接池
        /// </summary>
        public static void Dispose()
        {
            IDisposable pool = ConnectionSettings;
            pool.Dispose();
        }
    }

    /// <summary>
    /// just for example
    /// </summary>
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
