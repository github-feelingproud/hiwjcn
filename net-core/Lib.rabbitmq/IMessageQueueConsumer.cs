using System;

namespace Lib.mq
{
    public interface IMessageQueueConsumer : IMessageQueueConsumer<string> { }

    public interface IMessageQueueConsumer<T> : IDisposable
    {
        //
    }
}
