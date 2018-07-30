using Lib.extension;
using Lib.ioc;
using System;
using System.Linq;

namespace Lib.core
{
    /// <summary>
    /// 释放Lib库内所用占用的资源
    /// </summary>
    public static class LibReleaseHelper
    {
        public static void DisposeAll()
        {
            if (IocContext.Instance.Inited)
                using (var s = IocContext.Instance.Scope())
                {
                    //释放
                    var coms = s.ResolveAll<IDisposeComponent>();
                    foreach (var com in coms.OrderBy(x => x.Order))
                    {
                        try
                        {
                            com.Dispose();
                        }
                        catch (Exception e)
                        {
                            e.AddErrorLog(com.ComponentName);
                        }
                    }
                    //释放ioc中的对象
                    var disposes = s.ResolveAll<IDisposable>();
                    foreach (var dis in disposes)
                    {
                        try
                        {
                            dis.Dispose();
                        }
                        catch (Exception e)
                        {
                            e.AddErrorLog();
                        }
                    }
                }

            //回收内存
            GC.Collect();
        }
    }
}
