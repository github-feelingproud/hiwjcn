using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Lib.mq
{
    public class SettingConfig
    {
        public string ExchangeName { get; set; }

        public ExchangeType ExchangeType { get; set; } = ExchangeType.direct;

        public bool Delay { get; set; } = false;

        public string QueueName { get; set; }

    }

    public static class RabbitMQExtension
    {
        public static void QueueDeclareXX(this IModel channel)
        {
            channel.QueueDeclare();
        }

        /// <summary>
        /// 添加默认设置项
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="exchange_name"></param>
        /// <param name="exchange_type"></param>
        /// <param name="is_delay"></param>
        public static void X_ExchangeDeclare(this IModel channel,
            string exchange_name, ExchangeType exchange_type, bool is_delay = false)
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

        /// <summary>声明一个队列并将队列绑定到exchange</summary>
        /// <param name="queueName">队列名称</param>
        /// <param name="exchangeName">交换机名称</param>
        public static QueueDeclareOk QueueBind(this IModel channel, string queueName, string exchangeName) => QueueBind(channel, queueName, exchangeName, queueName, null);

        /// <summary>声明一个队列并将队列绑定到exchange</summary>
        /// <param name="queueName">队列名称</param>
        /// <param name="exchangeName">交换机名称</param>
        /// <param name="priority">优先级数量，priority + 1</param>
        public static QueueDeclareOk QueueBind(this IModel channel, string queueName, string exchangeName, MessagePriority priority) => QueueBind(channel, queueName, exchangeName, queueName, priority);

        /// <summary>声明一个队列并将队列绑定到exchange。一个routingKey绑定多个Queue，一个Queue绑定多个routingKey</summary>
        /// <param name="queueName">队列名称</param>
        /// <param name="exchangeName">交换机名称</param>
        /// <param name="routingKey">routingKey</param>
        public static QueueDeclareOk QueueBind(this IModel channel, string queueName, string exchangeName, string routingKey) => QueueBind(channel, queueName, exchangeName, routingKey, null);

        /// <summary>声明一个队列并将队列绑定到exchange。一个routingKey绑定多个Queue，一个Queue绑定多个routingKey</summary>
        /// <param name="queueName">队列名称</param>
        /// <param name="exchangeName">交换机名称</param>
        /// <param name="routingKey">routingKey</param>
        /// <param name="priority">优先级数量，priority + 1</param>
        public static QueueDeclareOk QueueBind(this IModel channel, string queueName, string exchangeName, string routingKey, MessagePriority priority) => QueueBind(channel, queueName, exchangeName, routingKey, priority < MessagePriority.Lowest ? null : new Dictionary<string, object>() { { "x-max-priority", priority > MessagePriority.Highest ? 10 : (byte)priority + 1 } });

        /// <summary>声明一个队列并将队列绑定到exchange。一个routingKey绑定多个Queue，一个Queue绑定多个routingKey</summary>
        /// <param name="queueName">队列名称</param>
        /// <param name="exchangeName">交换机名称</param>
        /// <param name="routingKey">routingKey</param>
        /// <param name="arguments">arguments</param>
        public static QueueDeclareOk QueueBind(this IModel channel, string queueName, string exchangeName, string routingKey, IDictionary<string, object> arguments)
        {
            var result = channel.QueueDeclare(queueName, true, false, false, arguments);
            channel.QueueBind(queueName, exchangeName, routingKey);

            return result;
        }

        /// <summary>
        /// 基本设置
        /// </summary>
        public static void BasicSetting(this IModel channel, SettingConfig config)
        {
            channel.X_ExchangeDeclare(config.ExchangeName, config.ExchangeType, config.Delay);
            //channel.QueueDeclare(config.QueueName,true,)
        }

    }
}
