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
    public static class PollyExtension
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

    }
}
