using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lib.core
{
    public static class TimeOutHelper
    {
        /// <summary>
        /// https://github.com/App-vNext/Polly
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="action"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public static TResult RunWithTimeout<TResult>(Func<TResult> action, TimeSpan delay)
        {
            using (var tokensource = new CancellationTokenSource())
            {
                tokensource.CancelAfter(delay);
                var t = Task.Run(() => { return action(); }, tokensource.Token);
                t.Wait(tokensource.Token);
                return t.Result;
            }
        }
    }
    /// <summary>
    /// 实现功能熔断
    /// </summary>
    public class CircuitBreaker
    {
        public readonly List<DateTime> ErrorList = new List<DateTime>();
        public int ExceptionCount { get; set; }
        public TimeSpan ExceptionInTime { get; set; }
        public TimeSpan BlockTime { get; set; }
        public DateTime BlockUntil { get; set; }
    }
    public static class CircuitBreakerErrorListExtension
    {
        public static void CheckOrThrow(this CircuitBreaker breaker)
        {

        }
    }
    public static class CircuitBreakerExecuteExtension
    { }
}
