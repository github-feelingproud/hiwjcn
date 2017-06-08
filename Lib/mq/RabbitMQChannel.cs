using System;
using System.Collections.Generic;
using RabbitMQ.Client;

namespace Lib.mq
{
    public abstract class RabbitMQChannel : IDisposable
    {
        protected RabbitMQChannel(IModel channel)
        {
            Channel = channel;
        }

        public IModel Channel { get; }

        #region ExchangeDeclare
        public void ExchangeDeclare(string exchangeName) => ExchangeDeclare(exchangeName, ExchangeType.direct);

        public void ExchangeDeclare(string exchangeName, bool isDelay) => ExchangeDeclare(exchangeName, ExchangeType.direct, false);

        public void ExchangeDeclare(string exchangeName, ExchangeType type) => ExchangeDeclare(exchangeName, type, false);

        public void ExchangeDeclare(string exchangeName, ExchangeType type, bool isDelay)
        {
            if (isDelay)
                Channel.ExchangeDeclare(exchangeName, "x-delayed-message", true, false, new Dictionary<string, object>() { { "x-delayed-type", GetExchangeTypeName(type) } });
            else
                Channel.ExchangeDeclare(exchangeName, GetExchangeTypeName(type), true, false, null);
        }

        private static string GetExchangeTypeName(ExchangeType type)
        {
            switch (type)
            {
                case ExchangeType.direct:
                    return RabbitMQ.Client.ExchangeType.Direct;
                case ExchangeType.fanout:
                    return RabbitMQ.Client.ExchangeType.Fanout;
                case ExchangeType.topic:
                    return RabbitMQ.Client.ExchangeType.Topic;
            }
            throw new NotSupportedException();
        }
        #endregion

        #region ExchangeDeclareHeaders
        /// <summary>对Header进行匹配https://lostechies.com/derekgreer/2012/05/29/rabbitmq-for-windows-headers-exchanges/</summary>
        public void ExchangeDeclareHeaders(string exchangeName, bool any, IDictionary<string, object> headers) => ExchangeDeclareHeaders(exchangeName, false, any, headers);

        /// <summary>对key进行模式匹配，比如ab.* 可以传递到所有ab.*的queue。https://lostechies.com/derekgreer/2012/05/29/rabbitmq-for-windows-headers-exchanges/</summary>
        public void ExchangeDeclareHeaders(string exchangeName, bool isDelay, bool any, IDictionary<string, object> headers)
        {
            var specs = new Dictionary<string, object>(headers);

            specs["x-match"] = any ? "any" : "all";

            if (isDelay)
            {
                specs["x-delayed-type"] = RabbitMQ.Client.ExchangeType.Headers;
                Channel.ExchangeDeclare(exchangeName, "x-delayed-message", true, false, specs);
            }
            else
                Channel.ExchangeDeclare(exchangeName, RabbitMQ.Client.ExchangeType.Headers, true, false, specs);
        }
        #endregion

        #region QueueBind
        /// <summary>声明一个队列并将队列绑定到exchange</summary>
        /// <param name="queueName">队列名称</param>
        /// <param name="exchangeName">交换机名称</param>
        public QueueDeclareOk QueueBind(string queueName, string exchangeName) => QueueBind(queueName, exchangeName, queueName, null);

        /// <summary>声明一个队列并将队列绑定到exchange</summary>
        /// <param name="queueName">队列名称</param>
        /// <param name="exchangeName">交换机名称</param>
        /// <param name="priority">优先级数量，priority + 1</param>
        public QueueDeclareOk QueueBind(string queueName, string exchangeName, MessagePriority priority) => QueueBind(queueName, exchangeName, queueName, priority);

        /// <summary>声明一个队列并将队列绑定到exchange。一个routingKey绑定多个Queue，一个Queue绑定多个routingKey</summary>
        /// <param name="queueName">队列名称</param>
        /// <param name="exchangeName">交换机名称</param>
        /// <param name="routingKey">routingKey</param>
        public QueueDeclareOk QueueBind(string queueName, string exchangeName, string routingKey) => QueueBind(queueName, exchangeName, routingKey, null);

        /// <summary>声明一个队列并将队列绑定到exchange。一个routingKey绑定多个Queue，一个Queue绑定多个routingKey</summary>
        /// <param name="queueName">队列名称</param>
        /// <param name="exchangeName">交换机名称</param>
        /// <param name="routingKey">routingKey</param>
        /// <param name="priority">优先级数量，priority + 1</param>
        public QueueDeclareOk QueueBind(string queueName, string exchangeName, string routingKey, MessagePriority priority) => QueueBind(queueName, exchangeName, routingKey, priority < MessagePriority.Lowest ? null : new Dictionary<string, object>() { { "x-max-priority", priority > MessagePriority.Highest ? 10 : (byte)priority + 1 } });

        /// <summary>声明一个队列并将队列绑定到exchange。一个routingKey绑定多个Queue，一个Queue绑定多个routingKey</summary>
        /// <param name="queueName">队列名称</param>
        /// <param name="exchangeName">交换机名称</param>
        /// <param name="routingKey">routingKey</param>
        /// <param name="arguments">arguments</param>
        public QueueDeclareOk QueueBind(string queueName, string exchangeName, string routingKey, IDictionary<string, object> arguments)
        {
            var result = Channel.QueueDeclare(queueName, true, false, false, arguments);
            Channel.QueueBind(queueName, exchangeName, routingKey);

            return result;
        }
        #endregion

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
            if (Channel != null)
            {
                try
                {
                    Channel.Close();
                }
                catch
                {
                }
                try
                {
                    Channel.Dispose();
                }
                catch
                {
                }
            }

            _disposed = true;
        }

        ~RabbitMQChannel()
        {
            Dispose(false);
        }
        #endregion
    }

    /// <summary>
    /// exchange类型
    /// </summary>
    [Serializable]
    public enum ExchangeType : byte
    {
        /// <summary>如果routingKey匹配，那么Message就会被传递到相应的queue中。http://blog.csdn.net/anzhsoft/article/details/19630147</summary>
        direct = 1,
        /// <summary>会向所有响应的queue广播。http://blog.csdn.net/anzhsoft/article/details/19617305</summary>
        fanout = 2,
        /// <summary>对key进行模式匹配，比如ab.* 可以传递到所有ab.*的queue。* (星号) 代表任意 一个单词；# (hash) 0个或者多个单词。http://blog.csdn.net/anzhsoft/article/details/19633079</summary>
        topic = 3,
        headers = 4
    }

    /// <summary>
    /// 优先级
    /// </summary>
    [Serializable]
    public enum MessagePriority : byte
    {
        /// <summary>优先级0</summary>
        None = 0,
        /// <summary>优先级1</summary>
        Lowest = 1,
        /// <summary>优先级2</summary>
        AboveLowest = 2,
        /// <summary>优先级3</summary>
        Low = 3,
        /// <summary>优先级4</summary>
        BelowNormal = 4,
        /// <summary>优先级5</summary>
        Normal = 5,
        /// <summary>优先级6</summary>
        AboveNormal = 6,
        /// <summary>优先级7</summary>
        Hight = 7,
        /// <summary>优先级8</summary>
        BelowHighest = 8,
        /// <summary>优先级9</summary>
        Highest = 9
    }
}
