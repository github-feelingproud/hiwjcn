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
    
}
