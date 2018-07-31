using Lib.extension;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.mq.rabbitmq
{
    /// <summary>
    /// rabitmq，配置
    /// </summary>
    public class RabbitMqSection
    {
        public static RabbitMqSection FromSection(string name) => throw new NotImplementedException();

        public string HostName { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string VirtualHost { get; set; }

        /// <summary>
        /// 默认3000ms
        /// </summary>
        public int ContinuationTimeout { get; set; }

        /// <summary>
        /// 默认2000ms
        /// </summary>
        public int SocketTimeout { get; set; }
    }
}
