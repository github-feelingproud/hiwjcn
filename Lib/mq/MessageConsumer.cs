using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lib.mq
{
    /// <summary>RabbitMQ消费者</summary>
    public abstract class RabbitMQConsumer : RabbitMQChannel
    {
        #region 属性

        /// <summary>ConsumerName</summary>
        public string ConsumerName { get; }

        /// <summary></summary>
        public Action<Exception> OnError;

        #endregion

        #region ctor

        internal RabbitMQConsumer(IModel channel) : base(channel)
        {
        }

        internal RabbitMQConsumer(IModel channel, string consumerName) : base(channel)
        {
            ConsumerName = consumerName;
        }

        /// <summary>声明一个临时队列，在Dispose之后将被删除</summary>
        /// <returns>临时队列名称</returns>
        public string TemporaryQueueBind(string exchangeName, string routingKey)
        {
            var ok = Channel.QueueDeclare();
            Channel.QueueBind(ok.QueueName, exchangeName, routingKey);
            return ok.QueueName;
        }

        #endregion

        #region Subscribe

        protected SemaphoreSlim Semaphore => _semaphore;
        protected CancellationTokenSource Cts => _cts;
        protected EventingBasicConsumer Consumer => _consumer;

        private SemaphoreSlim _semaphore;
        private CancellationTokenSource _cts; //取消标志
        private EventingBasicConsumer _consumer;

        protected void Subscribe(string queueName, ushort oncurrencySize, bool noAck, Action<BasicDeliverEventArgs> action)
        {
            if (_semaphore != null)
                throw new Exception("订阅已启动或未正常停止");

            _semaphore = new SemaphoreSlim(0);

            _cts = new CancellationTokenSource();

            Channel.BasicQos(0, oncurrencySize, false);

            _consumer = new EventingBasicConsumer(Channel);

            _consumer.Received += (sender, args) => action(args);

            Channel.BasicConsume(queueName, noAck, $"{Environment.MachineName}|{queueName}|{ConsumerName}", _consumer);
        }

        #endregion

        #region Dispose

        private bool _disposed;

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            base.Dispose(disposing);

            if (disposing)
            {
                if (_semaphore != null)
                {
                    _cts.Cancel();

                    while (_semaphore.CurrentCount > 0)
                    {
                        _semaphore.Wait(); //等待所有异步完成
                    }

                    _cts.Dispose();

                    _semaphore.Dispose();

                    _cts = null;
                    _semaphore = null;
                }
            }

            _disposed = true;
        }

        #endregion
    }

    /// <summary>Ack消费者</summary>
    public class RabbitMQAckConsumer : RabbitMQConsumer
    {
        #region ctor

        internal RabbitMQAckConsumer(IModel channel) : base(channel)
        {
        }

        internal RabbitMQAckConsumer(IModel channel, string consumerName) : base(channel, consumerName)
        {
        }

        #endregion

        #region Get

        /// <summary>尝试获取一条消息</summary>
        /// <param name="queueName">队列名称</param>
        /// <param name="onRecived">获取到</param>
        /// <returns>是否获取到消息</returns>
        public bool Get(string queueName, Func<BasicGetResult, bool> onRecived)
        {
            var msgResponse = Channel.BasicGet(queueName, false);
            if (msgResponse == null)
                return false;

            try
            {
                if (onRecived(msgResponse))
                    BasicAck(msgResponse.DeliveryTag);
                else
                    BasicNack(msgResponse.DeliveryTag);

                return true;
            }
            catch when (BasicNack(msgResponse.DeliveryTag))
            {
                throw;
            }
        }

        /// <summary>尝试获取一条消息</summary>
        /// <param name="queueName">队列名称</param>
        /// <param name="onRecivedAsync">获取到</param>
        /// <returns>是否获取到消息</returns>
        public async Task<bool> GetAsync(string queueName, Func<BasicGetResult, Task<bool>> onRecivedAsync)
        {
            var msgResponse = Channel.BasicGet(queueName, false);
            if (msgResponse == null)
                return false;

            try
            {
                if (await onRecivedAsync(msgResponse))
                    BasicAck(msgResponse.DeliveryTag);
                else
                    BasicNack(msgResponse.DeliveryTag);

                return true;
            }
            catch when (BasicNack(msgResponse.DeliveryTag))
            {
                throw;
            }
        }

        #endregion

        #region Subscribe

        /// <summary>需回复，同步回调</summary>
        /// <param name="queueName">队列名称</param>
        /// <param name="onRecived">回调</param>
        public void Subscribe(string queueName, Func<BasicDeliverEventArgs, bool> onRecived) => Subscribe(queueName, 1, onRecived);

        /// <summary>需回复，异步回调</summary>
        /// <param name="queueName">队列名称</param>
        /// <param name="concurrencySize">并发量</param>
        /// <param name="onRecived">回调，消息是否处理完成</param>
        public void Subscribe(string queueName, byte concurrencySize, Func<BasicDeliverEventArgs, bool> onRecived) =>
            Subscribe(queueName, concurrencySize, false, message => Task.Run(() => Consume(onRecived, message)));

        private void Consume(Func<BasicDeliverEventArgs, bool> onRecived, BasicDeliverEventArgs message)
        {
            Semaphore.Release();
            try
            {
                if (onRecived(message))
                    BasicAck(message.DeliveryTag);
                else
                    BasicNack(message.DeliveryTag);
            }
            catch (Exception ex)
            {
                BasicNack(message.DeliveryTag);

                OnError?.Invoke(ex);
            }
            finally
            {
                if (!Cts.IsCancellationRequested)
                    Semaphore.Wait();
            }
        }

        #endregion

        #region SubscribeAsync

        /// <summary>需回复，异步回调</summary>
        /// <param name="queueName">队列名称</param>
        /// <param name="onRecivedAsync">回调，消息是否处理完成</param>
        public void SubscribeAsync(string queueName, Func<BasicDeliverEventArgs, Task<bool>> onRecivedAsync) => SubscribeAsync(queueName, 1, onRecivedAsync);

        /// <summary>需回复，异步回调</summary>
        /// <param name="queueName">队列名称</param>
        /// <param name="concurrencySize">并发量</param>
        /// <param name="onRecivedAsync">回调</param>
        public void SubscribeAsync(string queueName, byte concurrencySize, Func<BasicDeliverEventArgs, Task<bool>> onRecivedAsync) =>
            Subscribe(queueName, concurrencySize, false, message => Task.Run(() => ConsumeAsync(onRecivedAsync, message)));

        private async Task ConsumeAsync(Func<BasicDeliverEventArgs, Task<bool>> onRecivedAsync, BasicDeliverEventArgs result)
        {
            Semaphore.Release();
            try
            {
                if (await onRecivedAsync(result))
                    BasicAck(result.DeliveryTag);
                else
                    BasicNack(result.DeliveryTag);
            }
            catch (Exception ex)
            {
                BasicNack(result.DeliveryTag);

                OnError?.Invoke(ex);
            }
            finally
            {
                if (!Cts.IsCancellationRequested)
                    Semaphore.Wait();
            }
        }

        #endregion

        #region Method

        private bool BasicNack(ulong deliveryTag)
        {
            Channel.BasicNack(deliveryTag, false, true);

            return false;
        }

        private bool BasicAck(ulong deliveryTag)
        {
            Channel.BasicAck(deliveryTag, false);

            return true;

            #endregion
        }
    }

    public class RabbitMQNoackConsumer : RabbitMQConsumer
    {
        #region ctor

        public RabbitMQNoackConsumer(IModel channel) : base(channel)
        {
        }

        public RabbitMQNoackConsumer(IModel channel, string consumerName) : base(channel, consumerName)
        {
        }

        #endregion

        /// <summary>尝试获取一条消息</summary>
        /// <param name="queueName">队列名称</param>
        /// <returns>是否获取到消息</returns>
        public BasicGetResult Get(string queueName)
        {
            var msgResponse = Channel.BasicGet(queueName, true);
            if (msgResponse == null)
                return null;

            return msgResponse;
        }

        #region Subscribe

        /// <summary>无需回复，同步回调</summary>
        /// <param name="queueName">队列名称</param>
        /// <param name="onRecived">回调。不管消息处理结果，都算处理完成</param>
        public void Subscribe(string queueName, Action<BasicDeliverEventArgs> onRecived) =>
            Subscribe(queueName, 1, true, message => Consume(onRecived, message));

        private void Consume(Action<BasicDeliverEventArgs> onRecived, BasicDeliverEventArgs message)
        {
            Semaphore.Release();
            try
            {
                onRecived(message);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
            }
            finally
            {
                if (!Cts.IsCancellationRequested)
                    Semaphore.Wait();
            }
        }

        #endregion

        #region SubscribeAsync

        /// <summary>无需回复，异步回调</summary>
        /// <param name="queueName">队列名称</param>
        /// <param name="onRecivedAsync">回调。不管消息处理结果，都算处理完成</param>
        public void SubscribeAsync(string queueName, Func<BasicDeliverEventArgs, Task> onRecivedAsync) =>
            Subscribe(queueName, 1, true, async message => await ConsumeAsync(onRecivedAsync, message));

        private async Task ConsumeAsync(Func<BasicDeliverEventArgs, Task> onRecivedAsync, BasicDeliverEventArgs message)
        {
            Semaphore.Release();
            try
            {
                await onRecivedAsync(message);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
            }
            finally
            {
                if (!Cts.IsCancellationRequested)
                    Semaphore.Wait();
            }
        }

        #endregion
    }
}