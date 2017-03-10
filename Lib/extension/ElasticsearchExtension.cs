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
using Lib.helper;

namespace Lib.extension
{
    public static class ElasticsearchExtension
    {
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
        /// 获取聚合，因为升级到5.0，这个方法不可用，还需要研究
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="response"></param>
        /// <returns></returns>
        private static Dictionary<string, List<KeyedBucket<string>>> GetAggs<T>(this ISearchResponse<T> response) where T : class, new()
        {
            return response?.Aggregations?.ToDictionary(
                    x => x.Key,
                    x => (x.Value as BucketAggregate)?.Items.Select(i => (i as KeyedBucket<string>)).Where(i => i?.DocCount > 0).ToList()
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
}
