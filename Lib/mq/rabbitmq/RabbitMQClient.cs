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
        private readonly IConnectionFactory _factory;
        private readonly Lazy<IConnection> _connection;

        public RabbitMQClient(string configurationName) : this(RabbitMQSection.FromSection(configurationName)) { }

        public RabbitMQClient(RabbitMQSection configuration)
        {
            this._factory = new ConnectionFactory
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

            _connection = new Lazy<IConnection>(() =>
            {
                var con = _factory.CreateConnection();
                con.ConnectionShutdown += (sender, args) => { };
                con.ConnectionBlocked += (sender, args) => { };
                con.ConnectionUnblocked += (sender, args) => { };
                return con;
            });
        }

        public IConnection Connection => _connection.Value;

        private bool _disposed = false;
        public void Dispose()
        {
            if (_disposed)
                return;

            try
            {
                this.Connection?.Close();
            }
            catch
            { }

            try
            {
                this.Connection?.Dispose();
            }
            catch
            { }

            _disposed = true;

            GC.SuppressFinalize(this);
        }

        ~RabbitMQClient()
        {
            this.Dispose();
        }
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
