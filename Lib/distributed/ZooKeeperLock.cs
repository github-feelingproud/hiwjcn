using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lib.extension;

namespace Lib.distributed
{
    public class ZooKeeperLock : DistributedLock
    {
        private readonly DistributedLockClient _client;
        private SemaphoreSlim _semaphore;

        public ZooKeeperLock(string key) : base(key)
        {
            _client = new DistributedLockClient(Key);

            _client.OnMasterChanged += _ => _semaphore?.Release();
        }

        public override bool Wait(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_semaphore != null)
                throw new Exception("锁释放前不能重复锁");

            _semaphore = new SemaphoreSlim(0, 1);

            var sw = Stopwatch.StartNew();

            _client.Lock();

            try
            {
                sw.Stop();

                //加200ms内耗
                if (_semaphore.Wait(Math.Max(0, millisecondsTimeout + 200 - (int)sw.ElapsedMilliseconds), cancellationToken))
                    return true;
            }
            catch (Exception e)
            {
                e.AddErrorLog();
                Release();
            }

            return false;
        }

        public override async Task<bool> WaitAsync(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_semaphore != null)
                throw new Exception("锁释放前不能重复锁");

            _semaphore = new SemaphoreSlim(0, 1);

            var sw = Stopwatch.StartNew();

            _client.Lock();

            try
            {
                sw.Stop();

                //加200ms内耗
                if (await _semaphore.WaitAsync(Math.Max(0, millisecondsTimeout + 200 - (int)sw.ElapsedMilliseconds), cancellationToken))
                    return true;
            }
            catch (Exception e)
            {
                e.AddErrorLog();
                Release();
            }

            return false;
        }

        public override void Release()
        {
            _semaphore?.Dispose();
            _semaphore = null;
            _client?.Release();
        }

        #region Dispose
        bool _disposed;
        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                //释放托管资源，比如将对象设置为null
            }

            //释放非托管资源
            Release();
            _client.Dispose();

            _disposed = true;
        }
        #endregion
    }
}
