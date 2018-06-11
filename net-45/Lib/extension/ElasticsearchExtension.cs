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
using Lib.data.elasticsearch;

namespace Lib.extension
{
    public static class ElasticsearchExtension
    {
        /// <summary>
        /// 如果有错误就抛出异常
        /// </summary>
        /// <param name="response"></param>
        public static T ThrowIfException<T>(this T response) where T : IResponse
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
            return response;
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

            var exist_response = client.IndexExists(indexName);
            exist_response.ThrowIfException();

            if (exist_response.Exists)
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
            var exist_response = await client.IndexExistsAsync(indexName);
            exist_response.ThrowIfException();

            if (exist_response.Exists)
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

            var exist_response = client.IndexExists(indexName);
            exist_response.ThrowIfException();

            if (!exist_response.Exists)
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
            var exist_response = await client.IndexExistsAsync(indexName);
            exist_response.ThrowIfException();

            if (!exist_response.Exists)
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
        /// https://elasticsearch.cn/article/142
        /// </summary>
        [Obsolete("只是为了演示用法")]
        public static SuggestDictionary<T> SuggestSample<T>(this IElasticClient client,
            string index,
            Expression<Func<T, object>> targetField, string text, string analyzer = null,
            string highlight_pre = "<em>", string hightlight_post = "</em>", int size = 20)
            where T : class, IElasticSearchIndex
        {
            var sd = new TermSuggesterDescriptor<T>();
            sd = sd.Field(targetField).Text(text);
            if (ValidateHelper.IsPlumpString(analyzer))
            {
                sd = sd.Analyzer(analyzer);
            }
            sd = sd.Size(size);

            new CompletionSuggesterDescriptor<T>();
            new PhraseSuggesterDescriptor<T>();

            var response = client.Search<T>(s => s.Suggest(ss => ss
    .Term("my-term-suggest", t => t
        .MaxEdits(1)
        .MaxInspections(2)
        .MaxTermFrequency(3)
        .MinDocFrequency(4)
        .MinWordLength(5)
        .PrefixLength(6)
        .SuggestMode(SuggestMode.Always)
        .Analyzer("standard")
        .Field("")
        .ShardSize(7)
        .Size(8)
        .Text("hello world")
    )
    .Completion("my-completion-suggest", c => c
        .Contexts(ctxs => ctxs
            .Context("color",
                ctx => ctx.Context("")
            )
        )
        .Fuzzy(f => f
            .Fuzziness(Fuzziness.Auto)
            .MinLength(1)
            .PrefixLength(2)
            .Transpositions()
            .UnicodeAware(false)
        )
        .Analyzer("simple")
        .Field("")
        .Size(8)
        .Prefix("")
    )
    .Phrase("my-phrase-suggest", ph => ph
        .Collate(c => c
            .Query(q => q
                .Source("{ \"match\": { \"{{field_name}}\": \"{{suggestion}}\" }}")
            )
            .Params(p => p.Add("field_name", "title"))
            .Prune()
        )
        .Confidence(10.1)
        .DirectGenerator(d => d
            .Field("")
        )
        .GramSize(1)
        .Field("")
        .Text("hello world")
        .RealWordErrorLikelihood(0.5)
    )
));

            response.ThrowIfException();

            return response.Suggest;
        }

        /// <summary>
        /// 给关键词添加高亮
        /// </summary>
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
        /// 唯一ID
        /// </summary>
        public static DocumentPath<T> ID<T>(this IElasticClient client, string indexName, string uid) where T : class, IElasticSearchIndex
        {
            return DocumentPath<T>.Id(uid).Index(indexName);
        }

        /// <summary>
        /// 判断文件是否存在
        /// </summary>
        public static bool DocExist_<T>(this IElasticClient client, string indexName, string uid) where T : class, IElasticSearchIndex
        {
            var response = client.DocumentExists(client.ID<T>(indexName, uid));
            return response.ThrowIfException().Exists;
        }

        /// <summary>
        /// 判断文件是否存在
        /// </summary>
        public static async Task<bool> DocExistAsync_<T>(this IElasticClient client, string indexName, string uid) where T : class, IElasticSearchIndex
        {
            var response = await client.DocumentExistsAsync(client.ID<T>(indexName, uid));
            return response.ThrowIfException().Exists;
        }

        /// <summary>
        /// 更新文档
        /// </summary>
        public static void UpdateDoc_<T>(this IElasticClient client, string indexName, string uid, T doc) where T : class, IElasticSearchIndex
        {
            var update_response = client.Update(client.ID<T>(indexName, uid), x => x.Doc(doc));
            update_response.ThrowIfException();
        }

        /// <summary>
        /// 更新文档
        /// </summary>
        public static async Task UpdateDocAsync_<T>(this IElasticClient client, string indexName, string uid, T doc) where T : class, IElasticSearchIndex
        {
            var update_response = await client.UpdateAsync(client.ID<T>(indexName, uid), x => x.Doc(doc));
            update_response.ThrowIfException();
        }

        /// <summary>
        /// 删除文档
        /// </summary>
        public static void DeleteDoc_<T>(this IElasticClient client, string indexName, string uid) where T : class, IElasticSearchIndex
        {
            var delete_response = client.Delete(client.ID<T>(indexName, uid));
            delete_response.ThrowIfException();
        }

        /// <summary>
        /// 删除文档
        /// </summary>
        public static async Task DeleteDocAsync_<T>(this IElasticClient client, string indexName, string uid) where T : class, IElasticSearchIndex
        {
            var delete_response = await client.DeleteAsync(client.ID<T>(indexName, uid));
            delete_response.ThrowIfException();
        }

        /// <summary>
        /// 通过条件删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="indexName"></param>
        /// <param name="where"></param>
        public static void DeleteByQuery_<T>(this IElasticClient client, string indexName, QueryContainer where) where T : class, IElasticSearchIndex
        {
            var query = new DeleteByQueryRequest<T>(indexName) { Query = where };

            var response = client.DeleteByQuery(query);

            response.ThrowIfException();
        }

        /// <summary>
        /// 通过条件删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="indexName"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public static async Task DeleteByQueryAsync_<T>(this IElasticClient client, string indexName, QueryContainer where) where T : class, IElasticSearchIndex
        {
            var query = new DeleteByQueryRequest<T>(indexName) { Query = where };

            var response = await client.DeleteByQueryAsync(query);

            response.ThrowIfException();
        }

        /// <summary>
        /// 根据距离排序
        /// </summary>
        public static SortDescriptor<T> SortByDistance<T>(this SortDescriptor<T> sort,
            Expression<Func<T, object>> field, GeoLocation point, bool desc = false) where T : class, IElasticSearchIndex
        {
            var geo_sort = new SortGeoDistanceDescriptor<T>().Points(point);
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
        [Obsolete("只是为了演示用法")]
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
            qc = qc && new GeoDistanceQuery()
            {
                Field = "Field Name",
                Location = new GeoLocation(32, 43),
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
            qc = qc && new GeoShapePolygonQuery()
            {
                Field = "xx",
                Shape = new PolygonGeoShape()
                {
                    Coordinates = new List<IEnumerable<GeoCoordinate>>()
                    {
                        new List<GeoCoordinate>() { }
                    }
                },
                Relation = GeoShapeRelation.Within
            };
            qc = qc && new GeoDistanceQuery()
            {
                Field = "Location",
                Location = new GeoLocation(32, 43),
                Distance = Distance.Kilometers(1)
            };
            qc &= new GeoShapeEnvelopeQuery()
            {
                Field = "Location",
                Shape = new EnvelopeGeoShape(new List<GeoCoordinate>() { }),
                Relation = GeoShapeRelation.Intersects,
            };
            qc &= new GeoShapePointQuery()
            {
                Field = "Location",
                Shape = new PointGeoShape(new GeoCoordinate(32, 32)),
                Relation = GeoShapeRelation.Intersects
            };
            qc &= new GeoShapeMultiPolygonQuery()
            {
                Field = "location",
                Shape = new MultiPolygonGeoShape(new List<List<List<GeoCoordinate>>>() { }) { },
                Relation = GeoShapeRelation.Intersects
            };
            //使用场景：一个销售区域支持多个配送闭环范围，查询当前位置在不在配送范围内
            var model = new
            {
                nested_sales_area = new object[]
                {
                    new
                    {
                        cordinates=new List<GeoCoordinate>(){ }
                    },
                    new
                    {
                        cordinates=new List<GeoCoordinate>(){ }
                    },
                }
            };
            var nested_query = new QueryContainer();
            nested_query &= new GeoShapePointQuery()
            {
                Field = "nested_sales_area.cordinates",
                Shape = new PointGeoShape(new GeoCoordinate(32, 32)),
                Relation = GeoShapeRelation.Intersects
            };
            qc &= new NestedQuery()
            {
                Path = "nested_sales_area",
                Query = nested_query
            };
        }

        [Obsolete("只是为了演示用法")]
        public static void HowToUseAggregationsInES(this SearchDescriptor<ProductListEsIndexModel> sd)
        {
            var agg = new AggregationContainer();
            agg = new SumAggregation("", "") && new AverageAggregation("", "");

            sd = sd.Aggregations(a => a.Max("max", x => x.Field(m => m.IsGroup))).Size(1000);
            sd = sd.Aggregations(a => a.Stats("stats", x => x.Field(m => m.BrandId).Field(m => m.PIsRemove)));
            //直方图
            sd = sd.Aggregations(a => a.Histogram("price", x => x.Field("price").Interval(60)));
            //时间直方图
            sd = sd.Aggregations(a => a.DateHistogram("date", x => x.Field("date").Interval(new Time(TimeSpan.FromHours(1)))));

            var response = ElasticsearchClientManager.Instance.DefaultClient.CreateClient().Search<ProductListEsIndexModel>(x => sd);

            var stats = response.Aggregations.Stats("stats");
            //etc
        }

        [Obsolete("只是为了演示用法")]
        public static void HowToSortWithScripts<T>(this SortDescriptor<T> sort) where T : class, IElasticSearchIndex
        {
            var sd = new SortScriptDescriptor<T>();

            sd = sd.Mode(SortMode.Sum).Type("number");
            var script = "doc['price'].value * params.factor";
            sd = sd.Script(x => x.Source(script).Lang("painless").Params(new Dictionary<string, object>()
            {
                ["factor"] = 1.1
            }));

            sort.Script(x => sd.Descending());
        }

        [Obsolete("只是为了演示用法")]
        public static void HowToUseNestedQuery(this QueryContainer qc)
        {
            var attr_list = new List<AttrParam>();
            if (ValidateHelper.IsPlumpList(attr_list))
            {
                var attr_query = new QueryContainer();
                foreach (var attr in attr_list)
                {
                    attr_query = attr_query || new TermQuery() { Field = $"ProductAttributes.{attr.UID}", Value = attr.value };
                }
                qc = qc && new NestedQuery()
                {
                    Path = "ProductAttributes",
                    Query = attr_query
                };
            }
        }

        [Obsolete("只是为了演示用法")]
        public static void HowToUseNestedSort(this SortDescriptor<ProductListEsIndexModel> sort)
        {
            sort = sort
           .Field(x => x.Field($"field.fieldxx")
           .Order(Nest.SortOrder.Descending)
           .Mode(SortMode.Max)
           .NestedPath("xxpath")
           //.NestedFilter(q => q.Term(m => m.Field("").Value(""))));
           .NestedFilter(q => q.Terms(m =>
           m.Field($"")
           .Terms(new List<string>() { })
           )));
        }

        /// <summary>
        /// query会计算匹配度（_score）
        /// filter不会计算匹配度，只会计算是否匹配，并且有查询缓存
        /// 不需要匹配度的查询使用filter效率更高
        /// </summary>
        [Obsolete("只是为了演示用法")]
        public static void DifferenceBetweenQueryAndFilter() { }

        [Obsolete("只是为了演示用法")]
        public static void DifferentQuerysInEs(this QueryContainer qc)
        {
            //匹配查询
            qc &= new MatchQuery()
            {
                Field = "analyized field name",
                Query = "关键词",
                Operator = Operator.Or,
                MinimumShouldMatch = "100%",
                Analyzer = "ik_smart"
            };

            //https://www.elastic.co/guide/en/elasticsearch/reference/current/query-dsl-query-string-query.html#query-string-syntax
            //https://www.elastic.co/guide/en/elasticsearch/reference/current/query-dsl-simple-query-string-query.html
            //query string自定义了一个查询语法
            new QueryStringQuery() { };

            //https://www.elastic.co/guide/cn/elasticsearch/guide/current/_wildcard_and_regexp_queries.html
            //使用通配符查询，比如name.*
            new WildcardQuery() { };

            //精准匹配，不分词
            new TermQuery() { };

            //https://www.elastic.co/guide/cn/elasticsearch/guide/current/fuzzy-query.html
            //模糊查询，它会计算关键词和目标字段的“距离”。如果在允许的距离范围内，计算拼写错误也可以匹配到
            new FuzzyQuery() { };

            //范围查询
            new DateRangeQuery() { };
            new NumericRangeQuery() { };
        }

        /// <summary>
        /// https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/function-score-query-usage.html
        /// </summary>
        /// <param name="sd"></param>
        [Obsolete("只是为了演示用法")]
        public static void HowToUseFunctionQuery(this SearchDescriptor<ProductListEsIndexModel> sd)
        {
            var qs = new FunctionScoreQuery()
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
                    new GaussGeoDecayFunction() { Origin=new GeoLocation(32,4) },
                    new RandomScoreFunction { Seed = "randomstring" },
                    new WeightFunction { Weight = 1.0},
                    new ScriptScoreFunction { Script = new ScriptQuery { Source = "x" } }
                }
            };
            sd = sd.Query(x => qs);
            sd = sd.Sort(x => x.Descending(s => s.UpdatedDate));
            sd = sd.Skip(0).Take(10);
            new ElasticClient().Search<ProductListEsIndexModel>(_ => sd);
        }

        [Obsolete("只是为了演示用法")]
        public static void HowToUseInnerAgg()
        {
            var sd = new SearchDescriptor<ProductListEsIndexModel>();
            sd = sd.Aggregations(agg => agg
                .Terms("NAMEOF_ShowCatalogIdList", av => av.Field("NAMEOF_ShowCatalogIdList").Size(1000))
                .Terms("NAMEOF_BrandId", av => av.Field("NAMEOF_BrandId").Order(x => x.CountDescending()).Size(1000))
                .Terms("NAMEOF_ProductAttributes",
                //妈的 这什么鬼
                av => av.Field("NAMEOF_ProductAttributes")
                .Aggregations(m => m.Average("", d => d.Field(""))).Order(xx => xx.Descending("")).Size(1000)));
        }

        /// <summary>
        /// https://stackoverflow.com/questions/42210930/nest-how-to-use-updatebyquery
        /// </summary>
        /// <param name="client"></param>
        [Obsolete("只是为了演示用法")]
        public static void HowToUpdateDocByScriptQuery(IElasticClient client)
        {
            var query = new QueryContainer();
            query &= new TermQuery() { Field = "name", Value = "wj" };

            client.UpdateByQuery<ProductListEsIndexModel>(q => q.Query(rq => query).Script(script => script
        .Source("ctx._source.name = newName;")
        .Params(new Dictionary<string, object>() { ["newName"] = "wj" })));

            //
            client.Update(DocumentPath<ProductListEsIndexModel>.Id(""),
                x => x.Index("").Type<ProductListEsIndexModel>().Doc(new ProductListEsIndexModel() { }));
        }

    }

    [Obsolete("只是为了演示用法")]
    [ElasticsearchType(IdProperty = "UKey", Name = "ProductList")]
    public class ProductListEsIndexModel : IElasticSearchIndex
    {
        [Text(Name = "UKey", Index = false)]
        public string UKey { get; set; }

        [Text(Name = "字段名", Index = false)]
        public string ProductId { get; set; }

        [Text(Name = "字段名", Index = false)]
        public string TraderId { get; set; }

        [Text(Name = "字段名", Index = false)]
        public string PlatformCatalogId { get; set; }

        [Text(Name = "字段名", Index = false)]
        public string BrandId { get; set; }

        [Number(Name = "PAvailability", Index = false)]
        public int PAvailability { get; set; }

        [Number(Name = "PIsRemove", Index = false)]
        public int PIsRemove { get; set; }

        [Number(Name = "UpAvailability", Index = false)]
        public int UpAvailability { get; set; }

        [Number(Name = "UpIsRemove", Index = false)]
        public int UpIsRemove { get; set; }

        [Number(Name = "IsGroup", Index = false)]
        public int IsGroup { get; set; }

        [Number(Name = "UpiId", Index = false)]
        public int UpiId { get; set; }

        /// <summary>
        /// 销量
        /// </summary>
        [Number(Name = "SalesVolume", Index = false)]
        public int SalesVolume { get; set; }

        /// <summary>
        /// 是否有货
        /// </summary>
        [Number(Name = "InventoryStatus", Index = false)]
        public int InventoryStatus { get; set; }


        [Number(Name = "SalesPrice", Index = false)]
        public decimal SalesPrice { get; set; }

        [Text(Name = "ShopName", Analyzer = "ik_max_word", SearchAnalyzer = "ik_max_word")]
        public string ShopName { get; set; }

        [Text(Name = "SeachTitle", Analyzer = "ik_max_word", SearchAnalyzer = "ik_max_word")]
        public string SeachTitle { get; set; }

        [Date(Name = "UpdatedDate")]
        public DateTime UpdatedDate { get; set; }

        [Text(Name = "字段名", Index = false)]
        public string[] ShowCatalogIdList { get; set; }

        [Text(Name = "字段名", Index = false)]
        public string[] PlatformCatalogIdList { get; set; }

        [Text(Name = "字段名", Index = false)]
        public string[] ProductAttributes { get; set; }

        [GeoPoint(Name = nameof(Location))]
        public GeoLocation Location { get; set; }

        [GeoShape(Name = nameof(Area))]
        public List<GeoLocation> Area { get; set; }
    }

    [Obsolete("只是为了演示用法")]
    public class AttrParam
    {
        public virtual string UID { get; set; }

        public virtual string value { get; set; }
    }
}
