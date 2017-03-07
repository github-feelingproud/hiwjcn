using Lib.helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.mq
{
    /// <summary>
    /// rabitmq，配置
    /// </summary>
    public class RabbitMQSection : ConfigurationSection
    {
        public static RabbitMQSection FromSection(string name)
        {
            return (RabbitMQSection)ConfigurationManager.GetSection(name);
        }

        [ConfigurationProperty(nameof(HostName), IsRequired = true)]
        public string HostName
        {
            get { return this[nameof(HostName)].ToString(); }
        }

        [ConfigurationProperty(nameof(UserName), IsRequired = true)]
        public string UserName
        {
            get { return this[nameof(UserName)].ToString(); }
        }

        [ConfigurationProperty(nameof(Password), IsRequired = false)]
        public string Password
        {
            get { return this[nameof(Password)].ToString(); }
        }

        [ConfigurationProperty(nameof(VirtualHost), IsRequired = false)]
        public string VirtualHost
        {
            get { return this[nameof(VirtualHost)].ToString(); }
        }

        /// <summary>默认3000ms</summary>
        [ConfigurationProperty(nameof(ContinuationTimeout), IsRequired = false)]
        public int ContinuationTimeout
        {
            get
            {
                return ConvertHelper.GetInt(this[nameof(ContinuationTimeout)], 3000);
            }
        }

        /// <summary>默认2000ms</summary>
        [ConfigurationProperty(nameof(SocketTimeout), IsRequired = false)]
        public int SocketTimeout
        {
            get
            {
                return ConvertHelper.GetInt(this[nameof(SocketTimeout)], 2000);
            }
        }
    }
}
