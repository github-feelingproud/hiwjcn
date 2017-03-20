using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.distributed
{
    public static class ZooKeeperConfigurationManager
    {
        private static readonly object Lock = new object();
        private static ZooKeeperConfigurationClient _configurationClient;

        public static Action OnConfigChanged;

        public static void Init(string path)
        {
            Init("zookeeper", path);
        }

        public static void Init(string configurationName, string path)
        {
            if (_configurationClient == null)
                lock (Lock)
                {
                    if (_configurationClient == null)
                    {
                        var client = new ZooKeeperConfigurationClient(configurationName, path);

                        client.ReinitConfig();

                        client.OnConfigChanged += () => OnConfigChanged();

                        _configurationClient = client;
                    }
                }
        }

        public static string GetAppSetting(string key) => _configurationClient.GetValue("AppSettings", key) ?? ConfigurationManager.AppSettings[key];

        public static string GetConnectionString(string name)
        {
            var connectionString = _configurationClient.GetValue("AppSettings", name);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                var con = ConfigurationManager.ConnectionStrings[name];
                if (con != null)
                    connectionString = con.ConnectionString;
            }

            return connectionString;
        }

        public static NameValueCollection GetValues(string key)
        {
            return _configurationClient.GetValues(key);
        }

        public static string GetValue(string section, string key)
        {
            return _configurationClient.GetValue(section, key);
        }

        public static void Dispose()
        {
            _configurationClient?.Dispose();
        }
    }
}
