using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Nest;
using System.Net;
using System.Configuration;
using Lib.extension;

namespace Lib.extension
{
    public static class ElasticsearchExtension
    {

    }
    public static class ElasticsearchHelper
    {
        //创建连接池Elasticsearch
        private static readonly ConnectionSettings ConnectionSettings;

        static ElasticsearchHelper()
        {
            ConnectionSettings = new ConnectionSettings(new StaticConnectionPool(ConfigurationManager.ConnectionStrings["Elasticsearch"].ConnectionString.Split('|', ';', ',').Select(s => new Uri(s))));

            ConnectionSettings.MaximumRetries(3);

            EnableDebug();
        }

        #region EnableDebug
        public static void EnableDebug() => EnableDebug(details =>
        {
            var request = $"{details.HttpMethod} {details.Uri}";

            if (details.RequestBodyInBytes != null)
                request += Environment.NewLine + Encoding.UTF8.GetString(details.RequestBodyInBytes);

            if (details.Success || details.HttpStatusCode == (int)HttpStatusCode.NotFound)
            {
                return;
            }

            request.AddBusinessWarnLog();

            if (details.ResponseBodyInBytes != null)
            {
                ("Elasticsearch error response: " + Encoding.UTF8.GetString(details.ResponseBodyInBytes)).AddBusinessWarnLog();
            }
        });

        public static void EnableDebug(Action<IApiCallDetails> handler)
        {
            ConnectionSettings.DisableDirectStreaming();
            ConnectionSettings.OnRequestCompleted(handler);
        }
        #endregion

        public static IElasticClient CreateClient() => new ElasticClient(ConnectionSettings);

        private static readonly Func<TypeMappingDescriptor<object>, TypeMappingDescriptor<object>> DefaultMapSelector = m => m.Dynamic()
            .DynamicTemplates(dt => dt
                .DynamicTemplate("template_string",
                    mdt => mdt
                        .Mapping(mdtm => mdtm.String(s => s.Index(FieldIndexOption.NotAnalyzed)))
                        .MatchMappingType("string")
                        .Match("*"))
                .DynamicTemplate("template_nested",
                    mdt => mdt
                        .Mapping(mdtm => mdtm.Nested<object>(n => n.AutoMap()))
                        .MatchMappingType("object")
                        .Match("*")));

        /// <summary>所有字符串NotAnalyzed，所有object都mapping成nested</summary>
        public static MappingsDescriptor MapDefault(this MappingsDescriptor md) =>
            md.Map("_default_", DefaultMapSelector);

        /// <summary>所有字符串NotAnalyzed，所有object都mapping成nested</summary>
        public static MappingsDescriptor MapDefault(this MappingsDescriptor md, Func<TypeMappingDescriptor<object>, ITypeMapping> selector) =>
            md.Map("_default_", m => selector(DefaultMapSelector(m)));

        #region CreateIndexIfNotExists 如果索引不存在则创建索引

        /// <summary>如果索引不存在则创建索引</summary>
        /// <returns>true：索引存在或创建成功；false：索引创建失败</returns>
        public static bool CreateIndexIfNotExists(this IElasticClient client, string indexName) =>
            CreateIndexIfNotExists(client, indexName, null);

        /// <summary>如果索引不存在则创建索引</summary>
        /// <returns>true：索引存在或创建成功；false：索引创建失败</returns>
        public static bool CreateIndexIfNotExists(this IElasticClient client, string indexName, Func<CreateIndexDescriptor, ICreateIndexRequest> selector)
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

        /// <summary>如果索引不存在则创建索引</summary>
        /// <returns>true：索引存在或创建成功；false：索引创建失败</returns>
        public static Task<bool> CreateIndexIfNotExistsAsync(this IElasticClient client, string indexName) =>
            CreateIndexIfNotExistsAsync(client, indexName, null);

        /// <summary>如果索引不存在则创建索引</summary>
        /// <returns>true：索引存在或创建成功；false：索引创建失败</returns>
        public static async Task<bool> CreateIndexIfNotExistsAsync(this IElasticClient client, string indexName, Func<CreateIndexDescriptor, ICreateIndexRequest> selector)
        {
            indexName = indexName.ToLower();

            if ((await client.IndexExistsAsync(indexName)).Exists)
                return true;

            var response = await client.CreateIndexAsync(indexName, selector);
            if (response.IsValid)
                return true;

            response.LogError();

            return false;
        }
        #endregion

        #region LogError

        public static void LogError(this IResponse response)
        {
            if (response.ServerError?.Error == null)
            {
                if (response.OriginalException != null)
                    response.OriginalException.AddErrorLog();

                return;
            }
        }
        #endregion
    }
}
