﻿using Lib.extension;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lib.mq.rabbitmq
{
    /// <summary>
    /// 测试ok
    /// </summary>
    public abstract class RabbitMqConsumerBase : IMessageQueueConsumer
    {
        private readonly IModel _channel;
        private readonly AsyncEventingBasicConsumer _consumer;
        private readonly ExchangeTypeEnum _exchange_type;
        private readonly string _exchange_name;
        private readonly string _queue_name;
        private readonly string _route_key;
        private readonly string _consumer_name;
        private readonly bool _ack;
        private readonly bool _delay;
        private readonly bool _persistent;
        private readonly ushort _concurrency_size;

        public RabbitMqConsumerBase(IModel channel, string consumer_name,
            string exchange_name, string queue_name, string route_key, ExchangeTypeEnum exchangeType,
            bool persistent, bool ack, ushort concurrency_size, bool delay)
        {
            this._channel = channel ?? throw new ArgumentNullException(nameof(channel));
            this._exchange_name = exchange_name ?? throw new ArgumentNullException(nameof(exchange_name));
            this._queue_name = queue_name ?? throw new ArgumentNullException(nameof(queue_name));
            this._route_key = route_key ?? throw new ArgumentNullException(nameof(route_key));
            this._consumer_name = consumer_name ?? throw new ArgumentNullException(nameof(consumer_name));
            this._ack = ack;
            this._concurrency_size = concurrency_size;
            this._delay = delay;
            this._exchange_type = exchangeType;
            this._persistent = persistent;

            this.SetupQueue();

            //异步消费
            this._consumer = new AsyncEventingBasicConsumer(this._channel);

            this.SetUpConsumer();

        }

        private void SetupQueue()
        {
            //exchange
            this._channel.ExchangeDeclare_(this._exchange_name, this._exchange_type,
                durable: this._persistent, auto_delete: false, is_delay: this._delay);
            //queue
            var queue_args = new Dictionary<string, object>() { };
            this._channel.QueueDeclare(this._queue_name, durable: this._persistent,
                exclusive: false, autoDelete: false, arguments: queue_args);
            //bind
            this._channel.QueueBind(this._queue_name, this._exchange_name, this._route_key);
            //qos
            this._channel.BasicQos(0, this._concurrency_size, false);
        }

        private void SetUpConsumer()
        {
            //consumer
            this._consumer.Received += async (sender, args) =>
            {
                try
                {
                    var result = await this.OnMessageReceived(sender, args);
                    if (this._ack && (result ?? false))
                    {
                        this._channel.BasicAck_(args);
                    }
                }
                catch (Exception e)
                {
                    //log errors
                    e.AddErrorLog($"无法消费");
                }
            };
            var consumerTag = $"{Environment.MachineName}|{this._queue_name}|{this._consumer_name}";
            this._channel.BasicConsume(
                queue: this._queue_name, autoAck: this._ack,
                consumerTag: consumerTag, consumer: this._consumer);
        }

        public abstract Task<bool?> OnMessageReceived(object sender, BasicDeliverEventArgs args);

        public void Dispose()
        {
            try
            {
                this._channel?.Close();
            }
            catch (Exception e)
            {
                e.AddErrorLog();
            }
            try
            {
                this._channel?.Dispose();
            }
            catch (Exception e)
            {
                e.AddErrorLog();
            }
        }
    }
}
