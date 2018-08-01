using Lib.ioc;
using System;

namespace Lib.core
{
    public interface IDisposeComponent : IDisposable
    {
        string ComponentName { get; }

        /// <summary>
        /// 按照asc排序释放资源
        /// </summary>
        int DisposeOrder { get; }
    }

    public class LibCoreDisposeComponent : IDisposeComponent
    {
        public string ComponentName => "lib core";

        public int DisposeOrder => int.MinValue;

        public void Dispose()
        {
            using (var s = IocContext.Instance.Scope())
            {
                //
            }
        }
    }
}
