using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lib.core
{
    /// <summary>
    /// 超时
    /// </summary>
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
    public static class RetryHelper
    {
        public static T Retry<T>(Func<T> action, int times = 3)
        {
            int i = 0;
            while (true)
            {
                ++i;
                try
                {
                    return action();
                }
                catch when (i <= times)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(Math.Pow(1.5, i - 5)));
                    //重试时间间隔
                    //[(0, 0.13168724279835392), (1, 0.19753086419753085), (2, 0.2962962962962963), 
                    //(3, 0.4444444444444444), (4, 0.6666666666666666), 
                    //(5, 1.0), (6, 1.5), (7, 2.25), (8, 3.375), (9, 5.0625)]
                }
            }
        }
    }
    /// <summary>
    /// 实现功能熔断
    /// </summary>
    public class CircuitBreaker
    {
        /// <summary>
        /// 多线程锁
        /// </summary>
        public readonly object locker = new object();

        /// <summary>
        /// 记录错误时间的list
        /// </summary>
        public readonly List<DateTime> ErrorList = new List<DateTime>();
        /// <summary>
        /// 最多允许的错误数
        /// </summary>
        public uint ExceptionCount { get; set; }
        /// <summary>
        /// 计算次时间内的错误数
        /// </summary>
        public uint ExceptionInSeconds { get; set; }
        /// <summary>
        /// 熔断时间
        /// </summary>
        public uint BlockSeconds { get; set; }
        /// <summary>
        /// 在此时间之前将直接抛出熔断异常
        /// </summary>
        public DateTime BlockUntil { get; set; }

        public CircuitBreaker(uint ExceptionCount, uint ExceptionInSeconds, uint BlockSeconds)
        {
            this.ExceptionCount = ExceptionCount;
            this.ExceptionInSeconds = ExceptionInSeconds;
            this.BlockSeconds = BlockSeconds;
            if (this.ExceptionCount <= 0)
            {
                throw new Exception($"{this.ExceptionCount}必须大于0");
            }
            if (this.ExceptionInSeconds <= 0)
            {
                throw new Exception($"{this.ExceptionInSeconds}必须大于0");
            }
            if (this.BlockSeconds <= 0)
            {
                throw new Exception($"{nameof(this.BlockSeconds)}必须大于0 ");
            }

            this.BlockUntil = DateTime.Now.AddSeconds(-1);
        }
    }
    /// <summary>
    /// 错误记录管理
    /// </summary>
    public static class CircuitBreakerErrorListExtension
    {
        public static void AddErrorLog(this CircuitBreaker breaker)
        {
            lock (breaker.locker)
            {
                var now = DateTime.Now;
                breaker.ErrorList.Add(now);
                //筛选时间段内的数据
                var query = breaker.ErrorList.Where(x => x > now.AddSeconds(-breaker.ExceptionInSeconds));
                //最多拿{breaker.ExceptionCount + 1}条记录
                query = query.OrderByDescending(x => x).Take((int)breaker.ExceptionCount + 1);
                //获取最新的错误记录
                var topError = query.ToList();
                //如果错误数过多，并且不在熔断状态就更新熔断时间
                if (topError.Count > breaker.ExceptionCount)
                {
                    if (now > breaker.BlockUntil)
                    {
                        //更新熔断时间
                        breaker.BlockUntil = now.AddSeconds(breaker.BlockSeconds);
                    }
                }
                //删除旧数据，保留最新数据
                breaker.ErrorList.Clear();
                breaker.ErrorList.AddRange(topError);
            }
        }
    }
    /// <summary>
    /// 执行用户action逻辑
    /// </summary>
    public static class CircuitBreakerExecuteExtension
    {
        public static void Execute(this CircuitBreaker breaker, Action action)
        {
            if (DateTime.Now < breaker.BlockUntil)
            {
                throw new Exception("当前操作被熔断");
            }
            try
            {
                action.Invoke();
            }
            catch (Exception e)
            {
                breaker.AddErrorLog();
                throw e;
            }
        }
    }
}
