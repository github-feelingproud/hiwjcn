using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.core;

namespace Lib.mq
{
    public class RabbitMQClient : IDisposable
    {
        private readonly IConnectionFactory _rabbitMqFactory;
        private readonly Lazy<IConnection> _rabbitMqConnection;

        #region ctor
        public RabbitMQClient(string configurationName) : this(RabbitMQSection.FromSection(configurationName)) { }
        public RabbitMQClient(RabbitMQSection configuration)
        {
            _rabbitMqFactory = new ConnectionFactory
            {
                AutomaticRecoveryEnabled = true,
                UseBackgroundThreadsForIO = true,
                HostName = configuration.HostName,
                UserName = configuration.UserName,
                Password = configuration.Password,
                VirtualHost = configuration.VirtualHost,
                ContinuationTimeout = TimeSpan.FromMilliseconds(configuration.ContinuationTimeout),
                HandshakeContinuationTimeout = TimeSpan.FromMilliseconds(configuration.ContinuationTimeout),
                RequestedConnectionTimeout = configuration.SocketTimeout,
                SocketReadTimeout = configuration.SocketTimeout,
                SocketWriteTimeout = configuration.SocketTimeout,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(1)
            };

            _rabbitMqConnection = new Lazy<IConnection>(() => _rabbitMqFactory.CreateConnection());
        }
        #endregion

        public IConnection Connection => _rabbitMqConnection.Value;

        #region Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                //释放托管资源，比如将对象设置为null
            }

            //释放非托管资源
            if (_rabbitMqConnection != null && _rabbitMqConnection.IsValueCreated)
            {
                //Connection.AutoClose = true;
                Connection.Close();
                Connection.Dispose();
            }

            _disposed = true;
        }

        //~RabbitMQClient()
        //{
        //    Dispose(false);
        //}
        #endregion

        /// <summary>创建生产者</summary>
        /// <param name="exchangeName">交换机名称</param>
        /// <returns>生产者</returns>
        public RabbitMQProducer CreateProducer(string exchangeName) => new RabbitMQProducer(Connection.CreateModel(), exchangeName);

        #region CreateConsumer
        /// <summary>创建Ack消费者</summary>
        public RabbitMQAckConsumer CreateConsumer() => new RabbitMQAckConsumer(Connection.CreateModel());

        /// <summary>创建Ack消费者</summary>
        /// <param name="consumerName">消费都名称后缀</param>
        public RabbitMQAckConsumer CreateConsumer(string consumerName) => new RabbitMQAckConsumer(Connection.CreateModel(), consumerName);

        /// <summary>创建Noack消费者</summary>
        public RabbitMQNoackConsumer CreateNoackConsumer() => new RabbitMQNoackConsumer(Connection.CreateModel());

        /// <summary>创建Noack消费者</summary>
        /// <param name="consumerName">消费都名称后缀</param>
        public RabbitMQNoackConsumer CreateNoackConsumer(string consumerName) => new RabbitMQNoackConsumer(Connection.CreateModel(), consumerName);
        #endregion
    }

    public class RabbitMQClientManager : StaticClientManager<RabbitMQClient>
    {
        public static readonly RabbitMQClientManager Instance = new RabbitMQClientManager();

        public override string DefaultKey
        {
            get
            {
                return "lib_rabbitmq";
            }
        }

        public override bool CheckClient(RabbitMQClient ins)
        {
            return ins != null && ins.Connection.IsOpen;
        }

        public override RabbitMQClient CreateNewClient(string key)
        {
            return new RabbitMQClient(key);
        }

        public override void DisposeClient(RabbitMQClient ins)
        {
            ins?.Dispose();
        }
    }
}
