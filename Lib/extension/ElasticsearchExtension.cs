using Elasticsearch.Net;
using Lib.helper;
using Nest;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using Lib.core;
using System.Threading.Tasks;

namespace Lib.extension
{
    public interface IElasticSearchIndex { }

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
                if (response.ServerError?.Error != null)
                {
                    var msg = $@"server errors:{response.ServerError.Error.ToJson()},debug information:{response.DebugInformation}";
                    throw new Exception(msg);
                }
                if (response.OriginalException != null)
                {
                    throw response.OriginalException;
                }
            }
        }

        /// <summary>
        /// 设置shards和replicas和model搜索deep
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="x"></param>
        /// <param name="shards"></param>
        /// <param name="replicas"></param>
        /// <param name="deep"></param>
        /// <returns></returns>
        public static CreateIndexDescriptor GetCreateIndexDescriptor<T>(this CreateIndexDescriptor x,
            int shards = 5, int replicas = 1, int deep = 5)
            where T : class, IElasticSearchIndex
        {
            return x.Settings(s =>
            s.NumberOfShards(shards).NumberOfReplicas(replicas)).Mappings(map => map.Map<T>(m => m.AutoMap(deep)));
        }

        /// <summary>
        /// 分页
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sd"></param>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        public static SearchDescriptor<T> QueryPage_<T>(this SearchDescriptor<T> sd, int page, int pagesize)
            where T : class, IElasticSearchIndex
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
        public static void CreateIndexIfNotExists(this IElasticClient client, string indexName, Func<CreateIndexDescriptor, ICreateIndexRequest> selector = null)
        {
            indexName = indexName.ToLower();

            if (client.IndexExists(indexName).Exists)
            {
                return;
            }

            var response = client.CreateIndex(indexName, selector);
            response.ThrowIfException();
        }

        /// <summary>
        /// 如果索引不存在就创建
        /// </summary>
        /// <param name="client"></param>
        /// <param name="indexName"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static async Task CreateIndexIfNotExistsAsync(this IElasticClient client, string indexName, Func<CreateIndexDescriptor, ICreateIndexRequest> selector = null)
        {
            indexName = indexName.ToLower();

            if ((await client.IndexExistsAsync(indexName)).Exists)
            {
                return;
            }

            var response = await client.CreateIndexAsync(indexName, selector);
            response.ThrowIfException();
        }

        /// <summary>
        /// 删除索引
        /// </summary>
        /// <param name="client"></param>
        /// <param name="indexName"></param>
        /// <returns></returns>
        public static void DeleteIndexIfExists(this IElasticClient client, string indexName)
        {
            indexName = indexName.ToLower();

            if (!client.IndexExists(indexName).Exists)
            {
                return;
            }

            var response = client.DeleteIndex(indexName);
            response.ThrowIfException();
        }

        /// <summary>
        /// 删除索引
        /// </summary>
        /// <param name="client"></param>
        /// <param name="indexName"></param>
        /// <returns></returns>
        public static async Task DeleteIndexIfExistsAsync(this IElasticClient client, string indexName)
        {
            indexName = indexName.ToLower();

            if (!(await client.IndexExistsAsync(indexName)).Exists)
            {
                return;
            }

            var response = await client.DeleteIndexAsync(indexName);
            response.ThrowIfException();
        }

        /// <summary>
        /// 添加到索引
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="indexName"></param>
        /// <param name="data"></param>
        public static void AddToIndex<T>(this IElasticClient client, string indexName, params T[] data)
            where T : class, IElasticSearchIndex
        {
            var bulk = new BulkRequest(indexName)
            {
                Operations = ConvertHelper.NotNullList(data).Select(x => new BulkIndexOperation<T>(x)).ToArray()
            };
            var response = client.Bulk(bulk);

            response.ThrowIfException();
        }

        /// <summary>
        /// 添加到索引
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="indexName"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static async Task AddToIndexAsync<T>(this IElasticClient client, string indexName, params T[] data) where T : class, IElasticSearchIndex
        {
            var bulk = new BulkRequest(indexName)
            {
                Operations = ConvertHelper.NotNullList(data).Select(x => new BulkIndexOperation<T>(x)).ToArray()
            };
            var response = await client.BulkAsync(bulk);

            response.ThrowIfException();
        }

        /// <summary>
        /// 搜索建议
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="targetField"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static IDictionary<string, Suggest[]> SuggestKeyword<T>(this IElasticClient client,
            Expression<Func<T, object>> targetField, string text)
            where T : class, IElasticSearchIndex
        {
            var response = client.Suggest<T>(
                x => x.Phrase("phrase_suggest",
                m => m.Field(targetField).Text(text)));

            response.ThrowIfException();

            return response.Suggestions;
        }

        /// <summary>
        /// 给关键词添加高亮
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sd"></param>
        /// <param name="pre"></param>
        /// <param name="after"></param>
        /// <param name="fieldHighlighters"></param>
        /// <returns></returns>
        public static SearchDescriptor<T> AddHighlightWrapper<T>(this SearchDescriptor<T> sd,
            string pre = "<em>", string after = "</em>",
            params Func<HighlightFieldDescriptor<T>, IHighlightField>[] fieldHighlighters)
            where T : class, IElasticSearchIndex
        {
            if (fieldHighlighters.Length <= 0)
            {
                throw new Exception("关键词高亮，但是没有指定高亮字段");
            }
            return sd.Highlight(x => x.PreTags(pre).PostTags(after).Fields(fieldHighlighters));
        }

        /// <summary>
        /// 获取高亮对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="re"></param>
        /// <returns></returns>
        public static List<HighlightHit> GetHighlights<T>(this ISearchResponse<T> re)
            where T : class, IElasticSearchIndex
        {
            var data = re.Highlights.Select(x => x.Value?.Select(m => m.Value).ToList()).ToList();
            data = data.Where(x => ValidateHelper.IsPlumpList(x)).ToList();

            return data.Reduce((a, b) => ConvertHelper.NotNullList(a).Concat(ConvertHelper.NotNullList(b)).ToList());
        }

        /// <summary>
        /// 获取聚合
        /// 升级到5.0，这个方法不可用，需要改动
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="response"></param>
        /// <returns></returns>
        public static Dictionary<string, List<KeyedBucket>> GetAggs<T>(this ISearchResponse<T> response)
            where T : class, IElasticSearchIndex
        {
            List<KeyedBucket> C(IAggregate x)
            {
                if (x is BucketAggregate _BucketAggregate)
                {
                    var data = new List<KeyedBucket>();
                    foreach (var i in _BucketAggregate.Items)
                    {
                        if (i is KeyedBucket _KeyedBucket)
                        {
                            if (_KeyedBucket.DocCount > 0)
                            {
                                data.Add(_KeyedBucket);
                            }
                        }
                    }
                    return data;
                }
                //老方式
                return (x as BucketAggregate)?.Items?.Select(i => (i as KeyedBucket)).Where(i => i?.DocCount > 0).ToList();
            }
            var aggs = response.Aggregations?.ToDictionary(x => x.Key, x => C(x.Value));
            return aggs.Where(x => ValidateHelper.IsPlumpList(x.Value)).ToDictionary(x => x.Key, x => x.Value);
        }

        /// <summary>
        /// 开启链接调试
        /// </summary>
        public static ConnectionSettings EnableDebug(this ConnectionSettings setting)
        {
            return setting.DisableDirectStreaming(true);
        }

        /// <summary>
        /// 记录请求信息
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="handlerOrDefault"></param>
        /// <returns></returns>
        public static ConnectionSettings LogRequestInfo(this ConnectionSettings pool, Action<IApiCallDetails> handlerOrDefault = null)
        {
            if (handlerOrDefault == null)
            {
                handlerOrDefault = x =>
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
            return pool.OnRequestCompleted(handlerOrDefault);
        }

        /// <summary>
        /// 创建client
        /// </summary>
        /// <param name="pool"></param>
        /// <returns></returns>
        public static IElasticClient CreateClient(this ConnectionSettings pool) => new ElasticClient(pool);

        /// <summary>
        /// 判断文件是否存在
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static bool DocExist_<T>(this IElasticClient client, string uid) where T : class, IElasticSearchIndex
        {
            var response = client.DocumentExists(DocumentPath<T>.Id(uid));
            response.ThrowIfException();
            return response.Exists;
        }

        /// <summary>
        /// 判断文件是否存在
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static async Task<bool> DocExistAsync_<T>(this IElasticClient client, string uid) where T : class, IElasticSearchIndex
        {
            var response = await client.DocumentExistsAsync(DocumentPath<T>.Id(uid));
            response.ThrowIfException();
            return response.Exists;
        }

        /// <summary>
        /// 更新文档
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="uid"></param>
        /// <param name="doc"></param>
        public static void UpdateDoc_<T>(this IElasticClient client, string uid, T doc) where T : class, IElasticSearchIndex
        {
            var update_response = client.Update(DocumentPath<T>.Id(uid), x => x.Doc(doc));
            update_response.ThrowIfException();
        }

        /// <summary>
        /// 更新文档
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="uid"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static async Task UpdateDocAsync_<T>(this IElasticClient client, string uid, T doc) where T : class, IElasticSearchIndex
        {
            var update_response = await client.UpdateAsync(DocumentPath<T>.Id(uid), x => x.Doc(doc));
            update_response.ThrowIfException();
        }

        /// <summary>
        /// 删除文档
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="uid"></param>
        public static void DeleteDoc_<T>(this IElasticClient client, string uid) where T : class, IElasticSearchIndex
        {
            var delete_response = client.Delete(DocumentPath<T>.Id(uid));
            delete_response.ThrowIfException();
        }

        /// <summary>
        /// 删除文档
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static async Task DeleteDocAsync_<T>(this IElasticClient client, string uid) where T : class, IElasticSearchIndex
        {
            var delete_response = await client.DeleteAsync(DocumentPath<T>.Id(uid));
            delete_response.ThrowIfException();
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
            Expression<Func<T, object>> field, double lat, double lng, bool desc = false)
            where T : class, IElasticSearchIndex
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
        /// ES空间搜索
        /// </summary>
        /// <param name="qc"></param>
        public static void HowToFilterByDistance(this QueryContainer qc)
        {
            qc = qc && new GeoBoundingBoxQuery()
            {
                Field = "name",
                BoundingBox = new BoundingBox()
                {
                    TopLeft = new GeoLocation(212, 32),
                    BottomRight = new GeoLocation(43, 56)
                }
            };
            qc = qc && new GeoDistanceRangeQuery()
            {
                Field = "Field Name",
                Location = new GeoLocation(32, 43),
                LessThanOrEqualTo = Distance.Kilometers(1)
            };
            qc = qc && new GeoShapeCircleQuery()
            {
                Field = "name",
                Shape = new CircleGeoShape()
                {
                    Coordinates = new GeoCoordinate(324, 535)
                },
                Relation = GeoShapeRelation.Within
            };
        }

        public static void HowToUseAggregationsInES(this SearchDescriptor<EsExample.ProductListV2> sd)
        {
            var agg = new AggregationContainer();
            agg = new SumAggregation("", Field.Create("")) && new AverageAggregation("", Field.Create(""));

            sd = sd.Aggregations(a => a.Max("max", x => x.Field(m => m.IsGroup)));
            sd = sd.Aggregations(a => a.Stats("stats", x => x.Field(m => m.BrandId).Field(m => m.PIsRemove)));

            var response = ElasticsearchClientManager.Instance.DefaultClient.CreateClient().Search<EsExample.ProductListV2>(x => sd);

            var stats = response.Aggs.Stats("stats");
            //etc
        }

        public static void SortWithScripts<T>(this SortDescriptor<T> sort) where T : class, IElasticSearchIndex
        {
            var sd = new SortScriptDescriptor<T>();

            sd = sd.Mode(SortMode.Sum);
            var script = "doc['price'].value * params.factor";
            sd = sd.Script(x => x.Inline(script).Lang("painless").Params(new Dictionary<string, object>()
            {
                ["factor"] = 1.1
            }));

            sort.Script(x => sd);
        }

        public static void FunctionQuery(this SearchDescriptor<EsExample.ProductListV2> sd)
        {
            new FunctionScoreQuery()
            {
                Name = "named_query",
                Boost = 1.1,
                Query = new MatchAllQuery { },
                BoostMode = FunctionBoostMode.Multiply,
                ScoreMode = FunctionScoreMode.Sum,
                MaxBoost = 20.0,
                MinScore = 1.0,
                Functions = new List<IScoreFunction>
                {
                    new ExponentialDecayFunction { Origin = 1.0, Decay =    0.5, Field = "", Scale = 0.1, Weight = 2.1 },
                    new GaussDateDecayFunction { Origin = DateMath.Now, Field = "", Decay = 0.5, Scale = TimeSpan.FromDays(1) },
                    new LinearGeoDecayFunction { Origin = new GeoLocation(70, -70), Field = "", Scale = Distance.Miles(1), MultiValueMode = MultiValueMode.Average },
                    new FieldValueFactorFunction
                    {
                        Field = "x", Factor = 1.1,    Missing = 0.1, Modifier = FieldValueFactorModifier.Ln
                    },
                    new RandomScoreFunction { Seed = 1337 },
                    new RandomScoreFunction { Seed = "randomstring" },
                    new WeightFunction { Weight = 1.0},
                    new ScriptScoreFunction { Script = new ScriptQuery { File = "x" } }
                }
            };
        }

        public static void UpdateDoc(IElasticClient client)
        {
            //https://stackoverflow.com/questions/42210930/nest-how-to-use-updatebyquery

            var query = new QueryContainer();
            query &= new TermQuery() { Field = "name", Value = "wj" };

            client.UpdateByQuery<EsExample.ProductListV2>(q => q.Query(rq => query).Script(script => script
        .Inline("ctx._source.name = newName;")
        .Params(new Dictionary<string, object>() { ["newName"] = "wj" })));

            //
            client.Update(DocumentPath<EsExample.ProductListV2>.Id(""),
                x => x.Index("").Type<EsExample.ProductListV2>().Doc(new EsExample.ProductListV2() { }));
        }

        public static void DeleteDoc(IElasticClient client)
        {
            //
            var query = new DeleteByQueryRequest<EsExample.ProductListV2>("index_name");

            query.Query = new QueryContainer();
            query.Query &= new TermQuery() { Field = "", Value = "" };

            client.DeleteByQuery(query);

            //
            client.Delete(DocumentPath<EsExample.ProductListV2>.Id(""));
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
            var pool = new ConnectionSettings(new StaticConnectionPool(urls)).MaximumRetries(2).EnableDebug();
            return pool;
        }

        public override void DisposeClient(ConnectionSettings ins)
        {
            IDisposable dis = ins;
            dis?.Dispose();
        }
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

                var mx = response.Aggs.Max("");

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
        public class ProductListV2 : IElasticSearchIndex
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

            [GeoPoint(Name = nameof(Location), LatLon = true, GeoHash = true, Validate = true)]
            public GeoLocation Location { get; set; }

            [GeoShape(Name = nameof(Area))]
            public GeoLocation Area { get; set; }
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







        public const string INDEX_NAME = "comment_index";

        public PagerData<CommentEs> QueryCommentFromEs(
            string user_product_id = null, string user_uid = null, string q = null, int page = 1, int pagesize = 10)
        {
            var data = new PagerData<CommentEs>();
            var client = ElasticsearchClientManager.Instance.DefaultClient.CreateClient();
            var temp = new CommentEs();
            var tag_temp = new TagEs();

            var sd = new SearchDescriptor<CommentEs>();
            sd = sd.Index(INDEX_NAME);

            #region where
            var query = new QueryContainer();
            if (ValidateHelper.IsPlumpString(user_product_id))
            {
                query &= new TermQuery() { Field = nameof(temp.UserProductUID), Value = user_product_id };
            }
            if (ValidateHelper.IsPlumpString(user_uid))
            {
                query &= new TermQuery() { Field = nameof(temp.UserUID), Value = user_uid };
            }
            if (ValidateHelper.IsPlumpString(q))
            {
                query &= new MatchQuery() { Field = nameof(temp.Comment), Query = q, Operator = Operator.Or, MinimumShouldMatch = "100%" };
            }
            sd = sd.Query(_ => query);
            #endregion

            #region order
            var sort = new SortDescriptor<CommentEs>();
            sort = sort.Descending(x => x.CreateTime);
            sd = sd.Sort(_ => sort);
            #endregion

            #region aggs
            sd = sd.Aggregations(x => x
            .Terms("tags", av => av.Field($"{nameof(temp.Tags)}.{nameof(tag_temp.TagName)}").Size(10))
            .Terms("shops", av => av.Field(f => f.TraderUID).Size(10))
            .Average("score", av => av.Field(f => f.Score)));
            #endregion

            #region pager
            sd = sd.QueryPage_(page, pagesize);
            #endregion

            var response = client.Search<CommentEs>(_ => sd);
            response.ThrowIfException();

            data.ItemCount = (int)response.Total;
            data.DataList = response.Documents.ToList();

            var tags_agg = response.Aggs.Terms("tags");
            var shops_agg = response.Aggs.Terms("shops");
            var score_agg = response.Aggs.Average("score");

            return data;
        }

        public void UpdateIndex()
        {
            var client = ElasticsearchClientManager.Instance.DefaultClient.CreateClient();
            client.CreateIndexIfNotExists(INDEX_NAME, x => x.GetCreateIndexDescriptor<CommentEs>());

            var ran = new Random((int)DateTime.Now.Ticks);
            var list = new List<CommentEs>() { };
            var comment_list = new List<string>()
            {
                "测试发表后页面刷新情况","获取AuthorizationCode报error=1是什么问题","GHOST博客怎么安装啊？急等！","我就测试一下有没有限制敏感词汇，我草你大爷","我看看了，我认为！这个IDE是Hbuilder","中国足协的最新处罚，足以体现“只许州官放火，不许百姓点灯”，桩桩事件的药引子自然都是裁判，他们最后又是事件最终的判官，能让所有球员都不畏惧权力吗？能让所有球员不将心中的愤懑发泄到对方球员身上吗？这就是具有中国特色的中超联赛，但愿足协的高官们，别再被舆论强奸了，真正给赛场以公平的环境，包括你们属下的裁判"
            };
            for (var i = 0; i < 10; ++i)
            {
                list.Add(new CommentEs()
                {
                    UID = Com.GetUUID(),
                    Comment = ran.Choice(comment_list),
                    CreateTime = DateTime.Now,
                    UpdateTime = DateTime.Now,
                    Images = ran.Sample(comment_list, 3).ToArray(),
                    Tags = new TagEs[]
                    {
                        new TagEs(){TagUID=Com.GetUUID(),TagName="技术交流" },
                        new TagEs(){TagUID=Com.GetUUID(),TagName="评论组件" }
                    },
                    TraderUID = Com.GetUUID(),
                    TraderName = Com.GetUUID(),
                    UserUID = Com.GetUUID(),
                    UserProductUID = Com.GetUUID(),
                    Score = ran.RealNext(10),
                    DimensionUID = Com.GetUUID()
                });
            }

            client.AddToIndex(INDEX_NAME, list.ToArray());
        }


        [ElasticsearchType(IdProperty = nameof(UID), Name = nameof(CommentEs))]
        public class CommentEs : IElasticSearchIndex
        {
            [String(Name = nameof(UID), Index = FieldIndexOption.NotAnalyzed)]
            public virtual string UID { get; set; }

            [String(Name = nameof(Comment), Analyzer = "ik_max_word", SearchAnalyzer = "ik_max_word")]
            public virtual string Comment { get; set; }

            [Date(Name = nameof(CreateTime))]
            public virtual DateTime CreateTime { get; set; }

            [Date(Name = nameof(UpdateTime))]
            public virtual DateTime UpdateTime { get; set; }

            [String(Name = nameof(Images), Index = FieldIndexOption.NotAnalyzed)]
            public virtual string[] Images { get; set; }

            [Object(Name = nameof(Tags))]
            public virtual TagEs[] Tags { get; set; }

            [String(Name = nameof(TraderUID), Index = FieldIndexOption.NotAnalyzed)]
            public virtual string TraderUID { get; set; }

            [String(Name = nameof(TraderName), Index = FieldIndexOption.NotAnalyzed)]
            public virtual string TraderName { get; set; }

            [String(Name = nameof(UserUID), Index = FieldIndexOption.NotAnalyzed)]
            public virtual string UserUID { get; set; }

            [String(Name = nameof(UserProductUID), Index = FieldIndexOption.NotAnalyzed)]
            public virtual string UserProductUID { get; set; }

            [Number(Name = nameof(Score))]
            public virtual double Score { get; set; }

            [String(Name = nameof(DimensionUID), Index = FieldIndexOption.NotAnalyzed)]
            public virtual string DimensionUID { get; set; }

        }

        public class TagEs
        {
            [String(Name = nameof(TagUID), Index = FieldIndexOption.NotAnalyzed)]
            public virtual string TagUID { get; set; }

            [String(Name = nameof(TagName), Index = FieldIndexOption.NotAnalyzed)]
            public virtual string TagName { get; set; }
        }
    }
}
