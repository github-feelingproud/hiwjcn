using Lib.mq;
using Microsoft.Extensions.DependencyInjection;

namespace Lib.rabbitmq
{
    public static class Bootstrap
    {
        /// <summary>
        /// 使用消息队列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        public static IServiceCollection UseMessageQueue<T>(this IServiceCollection collection)
            where T : class, IMessageQueueProducer =>
            collection.AddSingleton<IMessageQueueProducer, T>();
    }
}
