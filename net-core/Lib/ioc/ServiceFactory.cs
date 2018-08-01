using System;
using System.Collections.Concurrent;
using System.Text;
using System.Linq;
using Lib.extension;
using System.Collections.Generic;

namespace Lib.ioc
{
    public abstract class ServiceFactory<T> : ConcurrentDictionary<string, T>, IDisposable
        where T : IDisposable
    {
        public virtual T GetServices(string name)
        {
            return this[name];
        }

        public void Dispose()
        {
            foreach (var m in this.Values.AsEnumerable())
            {
                try
                {
                    using (m) { }
                }
                catch (Exception e)
                {
                    e.AddErrorLog();
                }
            }
            this.Clear();
        }
    }
}
