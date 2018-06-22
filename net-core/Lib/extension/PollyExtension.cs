using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.extension
{
    /// <summary>
    /// 重试
    /// </summary>
    public static class RetryExtension
    {
        #region 同步

        /// <summary>
        /// 重试多次
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="retryCount"></param>
        public static void InvokeWithRetry<T>(this Action action, int retryCount)
            where T : Exception
        {
            Policy.Handle<T>().Retry(retryCount).Execute(() => action.Invoke());
        }

        /// <summary>
        /// 重试+等待
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="retryCount"></param>
        /// <param name="interval"></param>
        public static void InvokeWithRetryAndWait<T>(this Action action, int retryCount, Func<int, TimeSpan> interval)
            where T : Exception
        {
            Policy.Handle<T>().WaitAndRetry(retryCount, interval).Execute(() => action.Invoke());
        }
        #endregion

        #region 异步

        /// <summary>
        /// 重试多次
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="retryCount"></param>
        public static async Task InvokeWithRetryAsync<T>(this Func<Task> action, int retryCount)
            where T : Exception
        {
            await Policy.Handle<T>().RetryAsync(retryCount).ExecuteAsync(async () => await action.Invoke());
        }

        /// <summary>
        /// 重试+等待
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="retryCount"></param>
        /// <param name="interval"></param>
        public static async Task InvokeWithRetryAndWaitAsync<T>(this Func<Task> action, int retryCount, Func<int, TimeSpan> interval)
            where T : Exception
        {
            await Policy.Handle<T>().WaitAndRetryAsync(retryCount, interval).ExecuteAsync(async () => await action.Invoke());
        }
        #endregion
    }

    /// <summary>
    /// 超时
    /// </summary>
    public static class TimeoutExtension
    {

        /// <summary>
        /// 超时将直接抛出异常
        /// </summary>
        public static void InvokeWithTimeOut(this Action action, int seconds, TimeoutStrategy? strategy = null)
        {
            strategy = strategy ?? TimeoutStrategy.Pessimistic;
            Policy.Timeout(seconds, strategy.Value).Execute(() =>
            {
                action();
            });
        }

    }

    /// <summary>
    /// 熔断
    /// </summary>
    public static class CircuitBreakerExtension
    {
        /// <summary>
        /// 是否是熔断状态
        /// </summary>
        /// <param name="breaker"></param>
        /// <returns></returns>
        public static bool IsBreak(this CircuitBreakerPolicy breaker)
        {
            return breaker.CircuitState != CircuitState.Closed;
        }

        /// <summary>
        /// 熔断，policy必须是全局的
        /// 线程安全：
        /// https://github.com/App-vNext/Polly/wiki/Circuit-Breaker#thread-safety-and-locking
        /// polly内部实现是线程安全的，但是如果你的委托不是线程安全的，那这个操作就不是线程安全的
        /// </summary>
        /// <param name="action"></param>
        /// <param name="policy"></param>
        public static void InvokeWithCircuitBreaker(this Action action, ref CircuitBreakerPolicy policy)
        {
            action.InvokeWithCircuitBreaker(policy);
        }

        /// <summary>
        /// 熔断，policy必须是全局的
        /// 线程安全：
        /// https://github.com/App-vNext/Polly/wiki/Circuit-Breaker#thread-safety-and-locking
        /// polly内部实现是线程安全的，但是如果你的委托不是线程安全的，那这个操作就不是线程安全的
        /// </summary>
        /// <param name="action"></param>
        /// <param name="policy"></param>
        public static void InvokeWithCircuitBreaker(this Action action, CircuitBreakerPolicy policy)
        {
            policy.Execute(() =>
            {
                action();
            });
        }

        /// <summary>
        /// 熔断，policy必须是全局的
        /// 线程安全：
        /// https://github.com/App-vNext/Polly/wiki/Circuit-Breaker#thread-safety-and-locking
        /// polly内部实现是线程安全的，但是如果你的委托不是线程安全的，那这个操作就不是线程安全的
        /// </summary>
        /// <param name="action"></param>
        /// <param name="policy"></param>
        public static async Task InvokeWithCircuitBreakerAsync(this Func<Task> action, CircuitBreakerPolicy policy)
        {
            await policy.ExecuteAsync(async () =>
            {
                await action();
            });
        }
    }
}
