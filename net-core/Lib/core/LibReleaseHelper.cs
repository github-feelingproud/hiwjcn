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
                    var coms = s.ResolveAll_<IDisposeComponent>();
                    foreach (var com in coms.OrderBy(x => x.DisposeOrder))
                    {
                        try
                        {
                            //dispose by using syntax
                            using (com) { }
                        }
                        catch (Exception e)
                        {
                            e.AddErrorLog(com.ComponentName);
                        }
                    }
                }

            //回收内存
            GC.Collect();
        }
    }
}
