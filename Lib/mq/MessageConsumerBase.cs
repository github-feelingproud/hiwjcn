using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.extension;
using Lib.helper;
using Lib.io;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Polly;

namespace Lib.mq
{
    public abstract class MessageConsumerBase : IDisposable
    {
        private IConnection _connection { get; set; }
        private IModel _channel { get; set; }
        private EventingBasicConsumer _consumer { get; set; }

        public MessageConsumerBase(ConnectionFactory factory, string exchange, string queue)
        {
            this._connection = factory.CreateConnection();
            this._channel = this._connection.CreateModel();
            
            this._channel.BasicSetting(new SettingConfig()
            {
                //
            });

            this._connection.ConnectionShutdown += (sender, args) => { };
            this._connection.ConnectionBlocked += (sender, args) => { };
            this._connection.ConnectionUnblocked += (sender, args) => { };
        }

        public abstract bool? OnMessageReceived(BasicDeliverEventArgs args);

        public void Dispose()
        {
            try
            {
                this._channel.Close();
                this._channel.Dispose();
            }
            catch (Exception e)
            {
                e.AddErrorLog();
            }
            try
            {
                this._connection.Close();
                this._connection.Dispose();
            }
            catch (Exception e)
            {
                e.AddErrorLog();
            }
        }
    }
}
