using Elasticsearch.Net;
using Lib.helper;
using Nest;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using Lib.core;

namespace Lib.extension
{
    public static class ElasticsearchExtension
    {
        /// <summary>
        /// 如果有错误就抛出异常
        /// </summary>
        /// <param name="response"></param>
        public static void ThrowIfException(this IResponse response)
        {
            if (!response.IsValid)
            {
                response.LogError();
                throw response.OriginalException;
            }
        }

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
        /// 分页
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sd"></param>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        public static SearchDescriptor<T> QueryPage_<T>(this SearchDescriptor<T> sd, int page, int pagesize) where T : class
        {
            var pager = PagerHelper.GetQueryRange(page, pagesize);
            return sd.Skip(pager.skip).Take(pager.take);
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
        /// 删除索引
        /// </summary>
        /// <param name="client"></param>
        /// <param name="indexName"></param>
        /// <returns></returns>
        public static bool DeleteIndexIfExists(this IElasticClient client, string indexName)
        {
            indexName = indexName.ToLower();

            if (!client.IndexExists(indexName).Exists)
                return true;

            var response = client.DeleteIndex(indexName);
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
        public static void AddToIndex<T>(this IElasticClient client, string indexName, params T[] data) where T : class
        {
            var bulk = new BulkRequest(indexName)
            {
                Operations = ConvertHelper.NotNullList(data).Select(x => new BulkIndexOperation<T>(x)).ToArray()
            };
            var response = client.Bulk(bulk);
            if (!response.IsValid)
            {
                response.LogError();
                throw new Exception("创建索引错误", response.OriginalException);
            }
        }

        /// <summary>
        /// 搜索建议
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="targetField"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static IDictionary<string, Suggest[]> SuggestKeyword<T>(this IElasticClient client, Expression<Func<T, object>> targetField, string text) where T : class
        {
            var response = client.Suggest<T>(
                x => x.Phrase("phrase_suggest",
                m => m.Field(targetField).Text(text)));

            if (!response.IsValid)
            {
                response.LogError();
                throw new Exception("建议错误", response.OriginalException);
            }
            return response.Suggestions;
        }

        /// <summary>
        /// 给关键词添加高亮
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sd"></param>
        /// <param name="pre"></param>
        /// <param name="after"></param>
        public static SearchDescriptor<T> AddHighlightWrapper<T>(this SearchDescriptor<T> sd, string pre = "<em>", string after = "</em>") where T : class
        {
            return sd.Highlight(x => x.PreTags(pre).PostTags(after));
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
            string server_error = null;
            if (response.ServerError?.Error != null)
            {
                server_error = "ES服务器错误";
            }
            if (response.OriginalException != null)
            {
                response.OriginalException.AddErrorLog(server_error);
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
                        method = x.HttpMethod.ToString()
                    }.ToJson().AddBusinessInfoLog();
                };
            }
            setting.DisableDirectStreaming();
            setting.OnRequestCompleted(handler);
        }

        /// <summary>
        /// 根据距离排序
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sort"></param>
        /// <param name="field"></param>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        /// <param name="desc"></param>
        /// <returns></returns>
        public static SortDescriptor<T> SortByDistance<T>(this SortDescriptor<T> sort,
            Expression<Func<T, object>> field, double lat, double lng, bool desc = false) where T : class
        {
            var geo_sort = new SortGeoDistanceDescriptor<T>().PinTo(new GeoLocation(lat, lng));
            if (desc)
            {
                geo_sort = geo_sort.Descending();
            }
            else
            {
                geo_sort = geo_sort.Ascending();
            }
            return sort.GeoDistance(x => geo_sort);
            /*
            if (desc)
            {
                return sort.GeoDistance(x => x.Field(field).PinTo(new GeoLocation(lat, lng)).Descending());
            }
            else
            {
                return sort.GeoDistance(x => x.Field(field).PinTo(new GeoLocation(lat, lng)).Ascending());
            }*/
        }

        /// <summary>
        /// 怎么通过距离筛选，请看源代码
        /// </summary>
        /// <param name="qc"></param>
        public static void HowToFilterByDistance(this QueryContainer qc)
        {
            qc = qc && new GeoDistanceRangeQuery()
            {
                Field = "Field Name",
                Location = new GeoLocation(32, 43),
                LessThanOrEqualTo = Distance.Kilometers(1)
            };
        }
    }

    /// <summary>
    /// ES服务器链接管理
    /// </summary>
    public class ElasticsearchClientManager : StaticClientManager<ConnectionSettings>
    {
        public static readonly ElasticsearchClientManager Instance = new ElasticsearchClientManager();

        public override string DefaultKey
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["ES"]?.ConnectionString;
            }
        }

        public override bool CheckClient(ConnectionSettings ins)
        {
            return ins != null;
        }

        public override ConnectionSettings CreateNewClient(string key)
        {
            var urls = key.Split('|', ';', ',').Select(s => new Uri(s));
            var ConnectionSettings = new ConnectionSettings(new StaticConnectionPool(urls));

            ConnectionSettings.MaximumRetries(2);

            return ConnectionSettings;
        }

        public override void DisposeClient(ConnectionSettings ins)
        {
            IDisposable dis = ins;
            dis?.Dispose();
        }
    }

    /// <summary>
    /// es
    /// https://www.elastic.co/products/elasticsearch
    /// </summary>
    public static class ElasticsearchHelper
    {
        /// <summary>
        /// 获取连接池
        /// </summary>
        /// <returns></returns>
        public static ConnectionSettings GetConnectionSettings() => ElasticsearchClientManager.Instance.DefaultClient;

        /// <summary>
        /// 获取链接对象
        /// </summary>
        /// <returns></returns>
        public static IElasticClient CreateClient() => new ElasticClient(ElasticsearchClientManager.Instance.DefaultClient);
    }

    /// <summary>
    /// ES例子
    /// </summary>
    public class EsExample
    {
        private QueryContainer BuildQuery(SearchParamModel model)
        {
            var temp = new ProductListV2();
            var qc = new QueryContainer();
            {
                var traderlist = new List<string>();
                if (!ValidateHelper.IsPlumpString(model.province))
                {
                    throw new Exception("缺少区域信息");
                }
                if (ValidateHelper.IsPlumpString(model.trader))
                {
                    if (traderlist.Contains(model.trader))
                    {
                        traderlist.Clear();
                        traderlist.Add(model.trader);
                    }
                    else
                    {
                        traderlist.Clear();
                    }
                }
                if (!ValidateHelper.IsPlumpList(traderlist))
                {
                    traderlist = new List<string>() { "构造一个不可能存在的值" };
                }
                qc = qc && new TermsQuery() { Field = nameof(temp.TraderId), Terms = traderlist };
            }
            var idlist = new string[] { };
            if (!new string[] { "2", "4" }.Contains(model.CustomerType))
            {

                qc = qc && (!new TermsQuery() { Field = nameof(temp.UKey), Terms = idlist });
            }
            else
            {
                qc = qc && (!new TermsQuery() { Field = nameof(temp.UKey), Terms = idlist });
            }
            if (ValidateHelper.IsPlumpString(model.brand))
            {
                var brand_sp = ConvertHelper.GetString(model.brand).Split(',').Where(x => ValidateHelper.IsPlumpString(x)).ToArray();
                qc = qc && new TermsQuery() { Field = nameof(temp.BrandId), Terms = brand_sp };
            }
            if (ValidateHelper.IsPlumpString(model.catalog))
            {
                qc = qc && (new TermQuery() { Field = nameof(temp.PlatformCatalogId), Value = model.catalog }
                || new TermsQuery() { Field = nameof(temp.PlatformCatalogIdList), Terms = new object[] { model.catalog } }
                || new TermsQuery() { Field = nameof(temp.ShowCatalogIdList), Terms = new object[] { model.catalog } });
            }
            if (model.min_price >= 0)
            {
                qc = qc && new NumericRangeQuery() { Field = nameof(temp.SalesPrice), GreaterThanOrEqualTo = (double)model.min_price };
            }
            if (model.max_price >= 0)
            {
                qc = qc && new NumericRangeQuery() { Field = nameof(temp.SalesPrice), LessThanOrEqualTo = (double)model.max_price };
            }

            new GeoDistanceQuery() { };
            qc = qc && new GeoDistanceRangeQuery()
            {
                Field = "Location",
                Location = new GeoLocation(32, 43),
                LessThanOrEqualTo = Distance.Kilometers(1)
            };

            try
            {
                if (!ValidateHelper.IsPlumpString(model.attr)) { model.attr = "[]"; }
                var attr_list = model.attr.JsonToEntity<List<AttrParam>>();
                /*
                 if (ValidateHelper.IsPlumpList(attr_list))
                {
                    var attr_query = new QueryContainer();
                    foreach (var attr in attr_list)
                    {
                        attr_query = attr_query || new TermQuery() { Field = $"{nameof(template.ProductAttributes)}.{attr.UID}", Value = attr.value };
                    }
                    qc = qc && new NestedQuery() { Path = nameof(template.ProductAttributes), Query = attr_query };
                }
                 */
                if (ValidateHelper.IsPlumpList(attr_list))
                {
                    //qc = qc && new TermsQuery() { Field = nameof(temp.ProductAttributes), Terms = attr_list.Select(attr => $"{attr.UID}@$@{attr.value}") };
                    foreach (var attr_key in attr_list.Select(x => x.UID).Distinct())
                    {
                        qc = qc && new TermsQuery() { Field = nameof(temp.ProductAttributes), Terms = attr_list.Where(x => x.UID == attr_key).Select(attr => $"{attr.UID}@$@{attr.value}") };
                    }
                }
            }
            catch { }
            if (model.isGroup)
            {
                qc = qc && new TermQuery() { Field = nameof(temp.IsGroup), Value = 1 };
            }
            if (ValidateHelper.IsPlumpString(model.qs))
            {
                qc = qc && (new MatchQuery() { Field = nameof(temp.ShopName), Query = model.qs, Operator = Operator.Or, MinimumShouldMatch = "100%" }
                || new MatchQuery() { Field = nameof(temp.SeachTitle), Query = model.qs, Operator = Operator.Or, MinimumShouldMatch = "100%" });
            }

            qc = qc && new TermQuery() { Field = nameof(temp.PAvailability), Value = 1 };
            qc = qc && new TermQuery() { Field = nameof(temp.UpAvailability), Value = 1 };
            qc = qc && new TermQuery() { Field = nameof(temp.PIsRemove), Value = 0 };
            qc = qc && new TermQuery() { Field = nameof(temp.UpIsRemove), Value = 0 };
            qc = qc && new NumericRangeQuery() { Field = nameof(temp.SalesPrice), GreaterThan = 0 };

            return qc;
        }

        private SortDescriptor<ProductListV2> BuildSort(SearchParamModel model)
        {
            var sort = new SortDescriptor<ProductListV2>();
            sort = sort.Descending(x => x.InventoryStatus);
            if (model.order_rule == "1")
            {
                sort = sort.Descending(x => x.SalesVolume);
            }
            else if (model.order_rule == "2")
            {
                sort = sort.Ascending(x => x.SalesPrice);
            }
            else
            {
                sort = sort.Descending(x => x.SalesPrice);
            }

            //更具坐标排序
            sort = sort.GeoDistance(x => x.Field(f => f.IsGroup).PinTo(new GeoLocation(52.310551, 4.404954)).Ascending());

            return sort;
        }

        private static readonly string ES_PRODUCTLIST_INDEX = ConfigurationManager.AppSettings["ES_PRODUCTLIST_INDEX"];
        private static readonly string ES_SERVERS = ConfigurationManager.AppSettings["ES_SERVERS"];

        private static readonly ConnectionSettings setting = new ConnectionSettings(new SniffingConnectionPool(ES_SERVERS.Split('|', ',').Select(x => new Uri(x)))).DisableDirectStreaming(true);

        private static void PrepareES(Func<ElasticClient, bool> func)
        {
            var client = new ElasticClient(setting);
            func.Invoke(client);
        }

        private static Dictionary<string, List<KeyedBucket>> GetAggs<T>(ISearchResponse<T> response) where T : class, new()
        {
            return response?.Aggregations?.ToDictionary(
                    x => x.Key,
                    x => (x.Value as BucketAggregate)?.Items.Select(i => (i as KeyedBucket)).Where(i => i != null).ToList()
                    )?.Where(x => ValidateHelper.IsPlumpList(x.Value)).ToDictionary(x => x.Key, x => x.Value);
        }

        public ISearchResponse<ProductListV2> SearchEsProducts(SearchParamModel model)
        {
            ISearchResponse<ProductListV2> response = null;
            var temp = new ProductListV2();
            PrepareES(client =>
            {
                var sd = new SearchDescriptor<ProductListV2>();

                sd = sd.Index(ES_PRODUCTLIST_INDEX);

                sd = sd.Query(q => BuildQuery(model));

                var NAMEOF_ShowCatalogIdList = nameof(temp.ShowCatalogIdList);
                var NAMEOF_BrandId = nameof(temp.BrandId);
                var NAMEOF_ProductAttributes = nameof(temp.ProductAttributes);

                sd = sd.Aggregations(agg => agg
                .Terms(NAMEOF_ShowCatalogIdList, av => av.Field(NAMEOF_ShowCatalogIdList).Size(1000))
                .Terms(NAMEOF_BrandId, av => av.Field(NAMEOF_BrandId).Size(1000))
                .Terms(NAMEOF_ProductAttributes, av => av.Field(NAMEOF_ProductAttributes).Size(1000)));

                sd = sd.Sort(x => BuildSort(model));

                //var range = PagerHelper.GetQueryRange(model.page, model.pagesize);
                //sd = sd.Skip(range[0]).Take(range[1]);
                sd = sd.QueryPage_(model.page, model.pagesize);

                response = client.Search<ProductListV2>(x => sd);

                return true;
            });
            if (response == null || !response.IsValid || response.OriginalException != null)
            {
                throw new Exception("ES 挂了");
            }
            return response;
        }

        public void SearchProducts(SearchParamModel model)
        {
            var data = new PagerData<object>();

            var response = SearchEsProducts(model);
            data.ItemCount = (int)(response?.Total ?? 0);
            var datalist = response?.Hits?.Select(x => x as Hit<ProductListV2>).Where(x => x != null).Select(x => x.Source).Where(x => x != null).ToList();

            //聚合的数据
            var aggs = GetAggs(response);
            data.DataList = ConvertHelper.NotNullList(data.DataList);
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

        public class SearchParamModel
        {
            public string qs { get; set; }
            public string trader { get; set; }
            public string brand { get; set; }
            public string catalog { get; set; }
            public string sys_coupon { get; set; }
            public string user_coupon { get; set; }
            public string attr { get; set; }
            public decimal min_price { get; set; }
            public decimal max_price { get; set; }
            public string province { get; set; }
            public string order_rule { get; set; }
            public int page { get; set; }
            public int pagesize { get; set; }
            public bool isSelf { get; set; }
            public bool isPost { get; set; }
            public bool isGroup { get; set; }
            public bool hidePrice { get; set; }
            public string CustomerType { get; set; }
            public string LoginUserID { get; set; }
        }

        public class AttrParam
        {
            public virtual string UID { get; set; }

            public virtual string value { get; set; }
        }
    }
}
