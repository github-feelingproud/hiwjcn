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
        int Order { get; }
    }

    public class LibCoreDisposeComponent : IDisposeComponent
    {
        public string ComponentName => "lib core";

        public int Order => int.MinValue;

        public void Dispose()
        {
            using (var s = IocContext.Instance.Scope())
            {
                //
            }
        }
    }
}
