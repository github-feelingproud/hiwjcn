using Lib.extension;
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

        /// <summary>
        /// 默认3000ms
        /// </summary>
        [ConfigurationProperty(nameof(ContinuationTimeout), IsRequired = false)]
        public int ContinuationTimeout
        {
            get => (this[nameof(ContinuationTimeout)]?.ToString() ?? "3000").ToInt();
        }

        /// <summary>
        /// 默认2000ms
        /// </summary>
        [ConfigurationProperty(nameof(SocketTimeout), IsRequired = false)]
        public int SocketTimeout
        {
            get => (this[nameof(SocketTimeout)]?.ToString() ?? "2000").ToInt();
        }
    }

    public class SettingConfig
    {
        public string ExchangeName { get; set; }

        public ExchangeTypeEnum ExchangeType { get; set; } = ExchangeTypeEnum.direct;

        public bool Delay { get; set; } = false;

        public bool Persistent { get; set; } = false;

        public string QueueName { get; set; }

        public string RouteKey { get; set; }

        public Dictionary<string, object> Args { get; set; }

        public bool Ack { get; set; } = true;

        public uint ConsumeRetryCount { get; set; } = 3;

        public uint ConsumeRetryWaitMilliseconds { get; set; } = 10;

        public string ConsumerName { get; set; }

        public int PreFetchSize { get; set; } = 0;

        public int PreFetchCount { get; set; } = 1;

        public int ExchangeSetting { get; set; }
        public int QueueSetting { get; set; }
    }

    public class MessageWrapper<T>
    {
        public int DeliverCount { get; set; } = 1;
        public T Data { get; set; }
    }
}
