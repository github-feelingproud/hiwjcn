using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lib.distributed
{
    public interface IDistributedLock : IDisposable
    {
        bool Wait(int millisecondsTimeout, CancellationToken cancellationToken);

        Task<bool> WaitAsync(int millisecondsTimeout, CancellationToken cancellationToken);

        void Release();
    }

    public static class DistributedLockExtension
    {
        #region Wait
        public static void Wait(this IDistributedLock @lock) => Wait(@lock, new CancellationToken());

        public static void Wait(this IDistributedLock @lock, CancellationToken cancellationToken) => @lock.Wait(Timeout.Infinite, cancellationToken);

        public static bool Wait(this IDistributedLock @lock, TimeSpan timeout) => Wait(@lock, timeout, new CancellationToken());

        public static bool Wait(this IDistributedLock @lock, TimeSpan timeout, CancellationToken cancellationToken)
        {
            // Validate the timeout
            var totalMilliseconds = (long)timeout.TotalMilliseconds;
            if (totalMilliseconds < -1 || totalMilliseconds > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(timeout), timeout, "Wait_TimeoutWrong");

            return @lock.Wait((int)timeout.TotalMilliseconds, cancellationToken);
        }

        public static bool Wait(this IDistributedLock @lock, int millisecondsTimeout) => @lock.Wait(millisecondsTimeout, new CancellationToken());
        #endregion

        #region WaitAsync
        public static Task WaitAsync(this IDistributedLock @lock) => WaitAsync(@lock, new CancellationToken());

        public static Task WaitAsync(this IDistributedLock @lock, CancellationToken cancellationToken) => @lock.WaitAsync(Timeout.Infinite, cancellationToken);

        public static Task<bool> WaitAsync(this IDistributedLock @lock, TimeSpan timeout) => WaitAsync(@lock, timeout, new CancellationToken());

        public static Task<bool> WaitAsync(this IDistributedLock @lock, TimeSpan timeout, CancellationToken cancellationToken)
        {
            var totalMilliseconds = (long)timeout.TotalMilliseconds;
            if (totalMilliseconds < -1 || totalMilliseconds > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(timeout), timeout, "Wait_TimeoutWrong");

            return @lock.WaitAsync((int)timeout.TotalMilliseconds, cancellationToken);
        }

        public static Task<bool> WaitAsync(this IDistributedLock @lock, int millisecondsTimeout) => @lock.WaitAsync(millisecondsTimeout, new CancellationToken());
        #endregion
    }

    public abstract class DistributedLock : IDistributedLock
    {
        public string Key { get; }

        protected DistributedLock(string key)
        {
            Key = key;
        }

        public abstract void Release();

        public abstract bool Wait(int millisecondsTimeout, CancellationToken cancellationToken);

        public abstract Task<bool> WaitAsync(int millisecondsTimeout, CancellationToken cancellationToken);

        #region Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);

        ~DistributedLock()
        {
            Dispose(false);
        }
        #endregion
    }

    public class DistributedLockException : Exception
    {
        public DistributedLockException(string message) : base(message)
        {
        }
    }
}
