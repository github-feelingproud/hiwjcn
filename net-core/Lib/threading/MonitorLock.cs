using System;
using System.Threading;

namespace Lib.threading
{
    public class MonitorLock : IDisposable
    {
        private readonly object _lock;

        public MonitorLock(object _lock)
        {
            this._lock = _lock ?? throw new ArgumentNullException(nameof(_lock));

            Monitor.Enter(this._lock);
        }

        public void Dispose()
        {
            if (this._lock != null)
                Monitor.Exit(this._lock);
        }
    }
}
