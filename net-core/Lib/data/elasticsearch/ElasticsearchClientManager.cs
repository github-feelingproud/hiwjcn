using Elasticsearch.Net;
using Lib.core;
using Lib.extension;
using Nest;
using System;
using System.Configuration;
using System.Linq;

namespace Lib.data.elasticsearch
{
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
}
