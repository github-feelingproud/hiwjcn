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

        public MessageConsumerBase(ConnectionFactory factory, SettingConfig config)
        {
            this._connection = factory.CreateConnection();
            this._connection.ConnectionShutdown += (sender, args) => { };
            this._connection.ConnectionBlocked += (sender, args) => { };
            this._connection.ConnectionUnblocked += (sender, args) => { };

            this._channel = this._connection.CreateModel();

            this._channel.BasicSetting(config);

            this._consumer = new EventingBasicConsumer(this._channel);
            this._consumer.Received += (sender, args) =>
            {
                try
                {
                    //重试策略
                    var retryPolicy = Policy.Handle<Exception>().Retry((int)config.ConsumeRetryCount);
                    retryPolicy.Execute(() =>
                    {
                        var result = this.OnMessageReceived(sender, args);
                        if (result == null || !result.Value)
                        {
                            throw new Exception("未能消费对象");
                        }
                    });
                    this._channel.X_BasicAck(args);
                }
                catch (Exception e)
                {
                    this._channel.X_BasicAck(args);
                    e.AddErrorLog();
                }
            };
            this._channel.BasicConsume(queue: config.QueueName, noAck: !config.Ack, consumer: this._consumer);
        }

        public abstract bool? OnMessageReceived(object sender, BasicDeliverEventArgs args);

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
