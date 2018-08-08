using System;

namespace Lib.ioc
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
        public LibCoreDisposeComponent(IServiceProvider provider)
        {
            //
        }

        public string ComponentName => "lib core";

        public int DisposeOrder => int.MinValue;

        public void Dispose()
        {
            //
        }
    }
}
