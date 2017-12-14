using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using Lib.extension;
using RabbitMQ.Client;

namespace Lib.mq
{
    public interface IMessageProducer
    {
        /// <summary>发送消息</summary>
        /// <param name="to">routingKey</param>
        /// <param name="message">消息</param>
        void Send(string to, object message);
        /// <summary>发送消息</summary>
        /// <param name="to">routingKey</param>
        /// <param name="message">消息</param>
        /// <param name="priority">优先级</param>
        void Send(string to, object message, MessagePriority priority);
        /// <summary>发送消息</summary>
        /// <param name="to">routingKey</param>
        /// <param name="message">消息</param>
        /// <param name="delay">延迟毫秒数</param>
        void Send(string to, object message, long delay);
        /// <summary>发送消息</summary>
        /// <param name="to">routingKey</param>
        /// <param name="message">消息</param>
        /// <param name="priority">优先级</param>
        /// <param name="delay">延迟毫秒数</param>
        void Send(string to, object message, MessagePriority priority, long delay);
    }

    /// <summary>消息发送者</summary>
    public class RabbitMQProducer : RabbitMQChannel, IMessageProducer
    {
        #region ctor
        internal RabbitMQProducer(IModel channel, string exchangeName) : base(channel)
        {
            ExchangeName = exchangeName;

            IsPersistent = true;
        }
        #endregion

        #region Properties

        /// <summary>是否持续化</summary>
        public bool IsPersistent { get; set; }

        /// <summary>ExchangeName</summary>
        public string ExchangeName { get; }

        /// <summary>重试次数，默认7次，总间隔约4.2秒。重试间隔使用的是1.5的指数，如，第一次间隔Math.Pow(1.5, -5)，第二次间隔Math.Pow(1.5, -4)</summary>
        public byte Retires { get; set; } = 7;
        #endregion

        #region IMessageProducer
        /// <summary>发送消息</summary>
        /// <param name="to">routingKey</param>
        /// <param name="message">消息</param>
        public virtual void Send(string to, object message) => Send(to, message, CreateBasicProperties(null, null));

        /// <summary>发送消息</summary>
        /// <param name="to">routingKey</param>
        /// <param name="message">消息</param>
        /// <param name="priority">优先级</param>
        public virtual void Send(string to, object message, MessagePriority priority) => Send(to, message, CreateBasicProperties(priority, null));

        /// <summary>发送消息</summary>
        /// <param name="to">routingKey</param>
        /// <param name="message">消息</param>
        /// <param name="delay">延迟毫秒数</param>
        public virtual void Send(string to, object message, long delay)
        {
            if (delay > 0)
                Send(to, message, new Dictionary<string, object> { { "x-delay", delay } });
            else
                Send(to, message);
        }

        /// <summary>发送消息</summary>
        /// <param name="to">routingKey</param>
        /// <param name="message">消息</param>
        /// <param name="priority">优先级</param>
        /// <param name="delay">延迟毫秒数</param>
        public virtual void Send(string to, object message, MessagePriority priority, long delay)
        {
            if (delay > 0)
                Send(to, message, priority, new Dictionary<string, object> { { "x-delay", delay } });
            else
                Send(to, message, priority);
        }
        #endregion

        #region Send
        /// <summary>发送消息</summary>
        public virtual void Send(string routingKey, object message, IDictionary<string, object> properties) => Send(routingKey, message, CreateBasicProperties(null, properties));

        /// <summary>发送消息</summary>
        public virtual void Send(string routingKey, object message, MessagePriority? priority, IDictionary<string, object> properties) => Send(routingKey, message, CreateBasicProperties(priority, properties));

        /// <summary>发送消息</summary>
        public virtual void Send(string routingKey, object message, long delay, IDictionary<string, object> properties) => Send(routingKey, message, null, delay, properties);

        /// <summary>发送消息</summary>
        public virtual void Send(string routingKey, object message, MessagePriority? priority, long delay, IDictionary<string, object> properties)
        {
            if (delay > 0)
            {
                var basicProperties = CreateBasicProperties(priority, properties);

                if (basicProperties.Headers == null)
                    basicProperties.Headers = new Dictionary<string, object>();

                basicProperties.Headers["x-delay"] = delay;

                Send(routingKey, message, basicProperties);
            }
            else
                Send(routingKey, message, properties);
        }

        public virtual IBasicProperties CreateBasicProperties(MessagePriority? priority, IDictionary<string, object> properties)
        {
            var basicProperties = Channel.CreateBasicProperties();

            if (IsPersistent)
                basicProperties.DeliveryMode = 2;
            else
                basicProperties.DeliveryMode = 1;

            if (priority != null)
            {
                if (priority > MessagePriority.Highest)
                    throw new ArgumentOutOfRangeException(nameof(priority), priority, "最大值为10");

                basicProperties.Priority = (byte)priority;
            }

            if (properties != null && properties.Count > 0)
                basicProperties.Headers = new Dictionary<string, object>(properties);

            return basicProperties;
        }


        /// <summary>发送消息</summary>
        public virtual void Send(string routingKey, object message, IBasicProperties basicProperties)
        {
            var counter = 0;
            while (true)
            {
                try
                {
                    var bs = Encoding.UTF8.GetBytes(message.ToJson());
                    Channel.BasicPublish(ExchangeName, routingKey, basicProperties, bs);

                    break;
                }
                catch when (counter < Retires)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(Math.Pow(1.5, counter++ - 5)));
                }
            }
        }

        #endregion
    }
}
