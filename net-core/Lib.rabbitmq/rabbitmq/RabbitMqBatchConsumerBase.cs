using Lib.extension;
using Lib.helper;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lib.mq.rabbitmq
{
    /// <summary>
    /// 批量消费
    /// </summary>
    public abstract class RabbitMqBatchConsumerBase : IDisposable
    {
        private readonly IConnection _connection;
        private readonly List<Task> _threads;
        private readonly int ConcurrencySize = 3;
        private readonly int BatchSize = 1000;
        private readonly TimeSpan BatchTimeout;
        private readonly TimeSpan? waitForDispose;
        private readonly string queue_name;
        private readonly bool auto_ack;

        private bool stop = false;


        public RabbitMqBatchConsumerBase()
        {
            //Task.Run(async () =>
            //{
            //    while (this._channel.IsOpen)
            //    {
            //        var now = DateTime.Now;
            //        if (((now - time) > span && list.Any()) || list.Count >= size)
            //        {
            //            //handle list
            //            var tp = new TaskCompletionSource<bool>();
            //            try
            //            {
            //                //await something
            //                tp.SetResult(true);
            //            }
            //            catch (Exception e)
            //            {
            //                tp.TrySetException(e);
            //            }

            //            await tp.Task.ConfigureAwait(false);

            //            //next
            //            list = new List<BasicGetResult>();
            //            time = now;
            //        }

            //        var data = this._channel.BasicGet("log", autoAck: true);
            //        if (data == null)
            //        {
            //            Thread.Sleep(100);
            //            continue;
            //        }
            //        list.Add(data);
            //    }
            //});
            //start threads
            this._threads = Com.Range(this.ConcurrencySize)
                .Select(x => Task.Run(this.FetchDataInBatch)).ToList();
        }

        public abstract void OnConsume(IReadOnlyList<BasicGetResult> data, Action<BasicGetResult, bool> ack_callback);

        /// <summary>
        /// 链接是共用的，通道是私有的
        /// </summary>
        /// <returns></returns>
        private IModel CreateChannel()
        {
            lock (this._connection)
                return this._connection.CreateModel();
        }

        private void AckMessage(IModel _channel, BasicGetResult x, bool success)
        {
            if (this.auto_ack)
                return;
            if (success)
            {
                _channel.BasicAck(x.DeliveryTag, true);
            }
            else
            {
                _channel.BasicNack(x.DeliveryTag, true, requeue: true);
            }
        }

        /// <summary>
        /// acquire multiple data from queue
        /// </summary>
        /// <returns></returns>
        private async Task FetchDataInBatch()
        {
            using (var _channel = this.CreateChannel())
            {
                var batch_data = new List<BasicGetResult>();
                var cursor = DateTime.Now;
                bool Timeout(DateTime now) => (cursor + this.BatchTimeout) >= now;

                while (_channel.IsOpen && !this.stop)
                {
                    var now = DateTime.Now;
                    if (batch_data.Count >= this.BatchSize || Timeout(now))
                    {
                        //consume
                        var d = batch_data.AsReadOnly();
                        this.OnConsume(d, (x, success) => this.AckMessage(_channel, x, success));
                        //batch consuming finished ,reset cursor
                        batch_data.Clear();
                        cursor = now;
                    }
                    //get data from queue
                    var data = _channel.BasicGet(queue_name, autoAck: auto_ack);
                    if (data == null)
                    {
                        //there is no data in queue,await 100 miliseconds
                        await 100;
                        continue;
                    }
                    batch_data.Add(data);
                }
            }
        }

        public void Dispose()
        {
            try
            {
                //require stop,instead of force those threads killed
                this.stop = true;
                if (this._threads?.Any() ?? false)
                {
                    if (this.waitForDispose != null)
                    {
                        //timeout mode
                        this._threads.Add(Task.Delay(this.waitForDispose.Value));
                        Task.WhenAny(this._threads).Wait();
                    }
                    else
                    {
                        //wait forever mode
                        Task.WhenAll(this._threads).Wait();
                    }
                }
            }
            catch (Exception e)
            {
                e.AddErrorLog();
            }
            try
            {
                this._connection?.Dispose();
            }
            catch (Exception e)
            {
                e.AddErrorLog();
            }
        }
    }
}
