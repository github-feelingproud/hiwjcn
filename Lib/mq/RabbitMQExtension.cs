using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using Lib.helper;
using RabbitMQ.Client.Events;
using Lib.extension;
using Polly;

namespace Lib.mq
{
    public class SettingConfig
    {
        public string ExchangeName { get; set; }

        public ExchangeTypeEnum ExchangeType { get; set; } = ExchangeTypeEnum.direct;

        public bool Delay { get; set; } = false;

        public string QueueName { get; set; }

        public string RouteKey { get; set; }

        public Dictionary<string, object> Args { get; set; }

        public bool Ack { get; set; } = true;

        public uint ConsumeRetryCount { get; set; } = 3;

        public uint ConsumeRetryWaitMilliseconds { get; set; } = 10;

        public string ConsumerName { get; set; }

        public int ExchangeSetting { get; set; }
        public int QueueSetting { get; set; }
    }

    public class MessageWrapper<T>
    {
        public int DeliverCount { get; set; } = 1;
        public T Data { get; set; }
    }

    public static class RabbitMQExtension
    {
        public static MessageWrapper<T> GetWrapperMessage<T>(this BasicDeliverEventArgs args)
        {
            return Encoding.UTF8.GetString(args.Body).JsonToEntity<MessageWrapper<T>>();
        }

        public static byte[] DataToWrapperMessageBytes<T>(this T data)
        {
            return Encoding.UTF8.GetBytes(new MessageWrapper<T>() { Data = data }.ToJson());
        }

        public static void X_BasicAck(this IModel channel, BasicDeliverEventArgs args)
        {
            channel.BasicAck(deliveryTag: args.DeliveryTag, multiple: false);
        }

        /// <summary>
        /// 添加默认设置项
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="exchange_name"></param>
        /// <param name="exchange_type"></param>
        /// <param name="is_delay"></param>
        public static void X_ExchangeDeclare(this IModel channel,
            string exchange_name, ExchangeTypeEnum exchange_type, bool is_delay = false)
        {
            if (is_delay)
            {
                channel.ExchangeDeclare(exchange_name, "x-delayed-message", true, false,
                    new Dictionary<string, object>()
                    {
                        ["x-delayed-type"] = exchange_type.ToString()
                    });
            }
            else
            {
                channel.ExchangeDeclare(exchange_name, exchange_type.ToString(), true, false, null);
            }
        }

        /// <summary>
        /// 添加默认设置项
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="queueName"></param>
        /// <param name="exchangeName"></param>
        /// <param name="routingKey"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public static QueueDeclareOk X_QueueBind(this IModel channel,
            string queueName, string exchangeName, string routingKey, IDictionary<string, object> arguments)
        {
            var queue = channel.QueueDeclare(queueName, true, false, false, arguments);
            channel.QueueBind(queueName, exchangeName, routingKey);

            return queue;
        }

        /// <summary>
        /// 基本设置
        /// </summary>
        public static void BasicSetting(this IModel channel, SettingConfig config)
        {
            channel.X_ExchangeDeclare(config.ExchangeName, config.ExchangeType, config.Delay);

            if (!ValidateHelper.IsPlumpString(config.QueueName)) { throw new ArgumentException(nameof(config.QueueName)); }
            channel.X_QueueBind(config.QueueName, config.ExchangeName, config.RouteKey, config.Args);

            //每个消费一次收到多少消息，其余的放在队列里
            channel.BasicQos(0, 1, false);
        }

        /// <summary>
        /// 发送队列
        /// </summary>
        public static void Send<T>(this IModel channel,
            string routeKey, T data,
            string exchangeName = "", uint retryCount = 5, Func<int, TimeSpan> sleepDurationProvider = null,
            bool save_to_disk = true)
        {
            sleepDurationProvider = sleepDurationProvider ?? (i => TimeSpan.FromMilliseconds(i * 100));
            var retryPolicy = Policy.Handle<Exception>().WaitAndRetry((int)retryCount, sleepDurationProvider);
            retryPolicy.Execute(() =>
            {
                //数据
                var wrapperdata = data.DataToWrapperMessageBytes();

                var properties = channel.CreateBasicProperties();
                properties.Priority = (byte)MessagePriority.Hight;
                properties.Persistent = save_to_disk;
                //etc
                //string exchange, string routingKey, IBasicProperties basicProperties, byte[] body
                channel.BasicPublish(exchangeName, routeKey, properties, wrapperdata);
            });
        }

    }
}
