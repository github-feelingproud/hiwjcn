using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lib.core
{
    /// <summary>
    /// 实现功能熔断
    /// </summary>
    public class CircuitBreaker
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
}
