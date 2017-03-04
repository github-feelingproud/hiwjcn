using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Nest;

namespace Tuhu.Service.Product.Server.SearchManager
{
    public class ElasticsearchProductSearchManager
    {
        private static readonly string IndexName = "productcatalog";

        public async void SelectPropertyValuesAsync()
        {
            var response = await new ElasticClient(ConnectionSettings)
            .SearchAsync<Product>(s => s.Index(IndexName)
                .Query(q => BuildQueryExpress(q, "", null))
                .Aggregations(agg => agg.Terms("", av => av.Field("").Size(1000)))
                .Size(0));
        }

        #region Static
        private static QueryContainer BuildQueryExpress(QueryContainerDescriptor<Product> q, string keyword, IDictionary<string, IEnumerable<string>> parameters)
        {
            var queries = new List<Func<QueryContainerDescriptor<Product>, QueryContainer>>();

            if (!string.IsNullOrWhiteSpace(keyword))
                queries.Add(qbm => qbm.Bool(qbmb => qbmb.Should(
                    qbs => qbs.Term(qbst => qbst
                        .Field(p => p.Pid)
                        .Value(keyword)),
                    qbs => qbs.Match(qbsm => qbsm
                        .Field(GetSearchProperty())
                        .Query(keyword)
                        .MinimumShouldMatch(ConfigurationManager.AppSettings["MinimumShouldMatch"])))));

            if (parameters?.Count > 0)
                queries.AddRange(parameters
                    .Select(property => new Func<QueryContainerDescriptor<Product>, QueryContainer>(qbm =>
                        qbm.Terms(qbmt => qbmt
                            .Field(property.Key)
                            .Terms(property.Value)))));

            return q.Bool(qb => qb.Must(queries));
        }

        private static Expression<Func<Product, object>> GetSearchProperty()
        {
            switch (ConfigurationManager.AppSettings["Analyzer"])
            {
                case "ik_max_word":
                    return p => p.KeywordForIkMax;
                case "ik_smart":
                    return p => p.KeywordForIkSmart;
                default:
                    return p => p.KeywordForStandard;
            }
        }

        //创建连接池Elasticsearch
        private static readonly ConnectionSettings ConnectionSettings = new ConnectionSettings(new SniffingConnectionPool(ConfigurationManager.ConnectionStrings["Elasticsearch"].ConnectionString.Split('|', ';', ',').Select(s => new Uri(s))));
        #endregion

        [ElasticsearchType(Name = "Product")]
        public class Product
        {
            public string Pid { get; set; }

            public string KeywordForIkMax { get; set; }

            public string KeywordForIkSmart { get; set; }

            public string KeywordForStandard { get; set; }
        }
    }
}