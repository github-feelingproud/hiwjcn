using Lib.extension;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace Lib.mq.rabbitmq
{
    public abstract class RabbitMqBatchConsumerBase
    {
        private IModel _channel;

        public RabbitMqBatchConsumerBase()
        {
            var list = new List<BasicGetResult>();
            var time = DateTime.Now;
            var span = TimeSpan.FromSeconds(30);
            var size = 500;
            Task.Run(async () =>
            {
                while (this._channel.IsOpen)
                {
                    var now = DateTime.Now;
                    if (((now - time) > span && list.Any()) || list.Count >= size)
                    {
                        //handle list
                        var tp = new TaskCompletionSource<bool>();
                        try
                        {
                            //await something
                            tp.SetResult(true);
                        }
                        catch (Exception e)
                        {
                            tp.TrySetException(e);
                        }

                        await tp.Task.ConfigureAwait(false);

                        //next
                        list = new List<BasicGetResult>();
                        time = now;
                    }

                    var data = this._channel.BasicGet("log", autoAck: true);
                    if (data == null)
                    {
                        Thread.Sleep(100);
                        continue;
                    }
                    list.Add(data);
                }
            });
        }
    }
}
