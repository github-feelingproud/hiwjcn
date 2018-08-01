using System;
using System.Threading.Tasks;

namespace Lib.distributed
{
    public interface IDistributedLock : IDisposable
    {
        Task LockOrThrow();
        Task ReleaseLock();
    }
}
