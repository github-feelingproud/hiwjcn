using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.distributed
{
    /// <summary>从ZooKeeper读取配置</summary>
    public class ZooKeeperConfigurationClient : ZooKeeperClient
    {
        private readonly string _path;
        private IDictionary<string, IDictionary<string, string>> _configSource;

        #region ctor
        /// <summary>ctor</summary>
        /// <param name="configurationName">配置节点名称，默认zookeeper</param>
        /// <param name="path">路径</param>
        public ZooKeeperConfigurationClient(string configurationName, string path) : base(configurationName)
        {
            _path = path;

            OnDataChanged += client => ReinitConfig();
        }
        #endregion

        public Action OnConfigChanged;

        internal void ReinitConfig()
        {
            _configSource = GetData<IDictionary<string, IDictionary<string, string>>>(_path, true);
            OnConfigChanged();
        }

        public string GetValue(string section, string key)
        {
            return _configSource != null && _configSource.ContainsKey(section) && _configSource[section].ContainsKey(key) ? _configSource[section][key] : null;
        }

        public NameValueCollection GetValues(string section)
        {
            var values = new NameValueCollection();
            if (_configSource != null && _configSource.ContainsKey(section))
                foreach (var item in _configSource[section])
                {
                    values[item.Key] = item.Value;
                }
            return values;
        }
    }
}
