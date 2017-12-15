using Lib.core;
using Lib.extension;
using Lib.mvc.plugin;
using Lib.threading;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Compilation;

[assembly: PreApplicationStartMethod(typeof(PluginManager), nameof(PluginManager.LoadPlugins))]
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
                    if (IsAlreadyLoaded(dll)) { continue; }

                    var ass = Assembly.LoadFile(dll.FullName);
                    BuildManager.AddReferencedAssembly(ass);
                }

                //如果出现重复引用的问题就清理解决方案，然后重新生成插件项目
                //var list = AppDomain.CurrentDomain.GetAssemblies().Select(x => x.FullName?.Split(',')?.FirstOrDefault()?.Trim()).ToList();
                //var repeat = list.GroupBy(x => x).Select(x => new { key = x, count = x.Count() }).Where(x => x.count > 1).ToList();

            }
        }

        /// <summary>
        /// Indicates whether assembly file is already loaded
        /// </summary>
        /// <param name="fileInfo">File info</param>
        /// <returns>Result</returns>
        private static bool IsAlreadyLoaded(FileInfo fileInfo)
        {
            //compare full assembly name
            //var fileAssemblyName = AssemblyName.GetAssemblyName(fileInfo.FullName);
            //foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            //{
            //    if (a.FullName.Equals(fileAssemblyName.FullName, StringComparison.InvariantCultureIgnoreCase))
            //        return true;
            //}
            //return false;

            //do not compare the full assembly name, just filename
            try
            {
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileInfo.FullName);
                if (fileNameWithoutExt == null)
                    throw new Exception($"Cannot get file extension for {fileInfo.Name}");
                foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
                {
                    string assemblyName = a.FullName.Split(new char[] { ',' }).FirstOrDefault();
                    if (fileNameWithoutExt.Equals(assemblyName, StringComparison.InvariantCultureIgnoreCase))
                        return true;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Cannot validate whether an assembly is already loaded. " + e.GetInnerExceptionAsJson());
                e.AddErrorLog();
            }
            return false;
        }
    }
}
