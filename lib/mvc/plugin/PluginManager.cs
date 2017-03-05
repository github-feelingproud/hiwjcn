using Lib.core;
using Lib.mvc.plugin;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Compilation;

[assembly: PreApplicationStartMethod(typeof(PluginManager), "LoadPlugins")]
namespace Lib.mvc.plugin
{
    /// <summary>
    /// Contributor: Umbraco (http://www.umbraco.com). Thanks a lot! 
    /// SEE THIS POST for full details of what this does - 
    /// http://shazwazza.com/post/Developing-a-plugin-framework-in-ASPNET-with-medium-trust.aspx
    /// </summary>
    public class PluginManager
    {
        private const string PluginsPath = "~/App_Data/Plugins/";

        private static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();

        /// <summary>
        /// 在执行application_start之前执行这个函数加载插件
        /// 要防止加载重复的dll
        /// </summary>
        public static void LoadPlugins()
        {
            if (!ConfigHelper.Instance.LoadPlugin) { return; }
            using (new WriteLockDisposable(Locker))
            {
                var PluginDir = ServerHelper.GetMapPath(PluginsPath);
                if (!Directory.Exists(PluginDir))
                {
                    throw new Exception("插件目录不存在");
                }
                var dlls = Directory.GetFiles(PluginDir, "*.dll", SearchOption.AllDirectories).Select(x => new FileInfo(x)).ToList();
                foreach (var dll in dlls)
                {
                    var allass = AppDomain.CurrentDomain.GetAssemblies().Select(x => x.FullName.Split(',').FirstOrDefault()).ToList();
                    var fileNameWithoutExt = Path.GetFileNameWithoutExtension(dll.FullName);

                    var nextDll = false;
                    foreach (var a in allass)
                    {
                        if (fileNameWithoutExt?.ToLower()?.Trim() == a?.ToLower()?.Trim())
                        {
                            nextDll = true;
                            break;
                        }
                    }
                    if (nextDll) { continue; }

                    var ass = Assembly.LoadFile(dll.FullName);
                    BuildManager.AddReferencedAssembly(ass);
                }

                //如果出现重复引用的问题就清理解决方案，然后重新生成插件项目
                //var list = AppDomain.CurrentDomain.GetAssemblies().Select(x => x.FullName?.Split(',')?.FirstOrDefault()?.Trim()).ToList();
                //var repeat = list.GroupBy(x => x).Select(x => new { key = x, count = x.Count() }).Where(x => x.count > 1).ToList();

            }
        }
    }
}
