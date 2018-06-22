﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lib.distributed
{
    public interface IDistributedLock : IDisposable
    {
        Task LockOrThrow();
        Task ReleaseLock();
    }
}
