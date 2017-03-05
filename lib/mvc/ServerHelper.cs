using Lib.helper;
using System;
using System.IO;
using System.Security;
using System.Web;
using System.Web.Hosting;

namespace Lib.mvc
{
    public static class ServerHelper
    {
        /// <summary>
        /// 在请求上下文中缓存对象,不能缓存null对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="func"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static T CacheInHttpContext<T>(string key, Func<T> func, HttpContext context = null)
            where T : class
        {
            context = context ?? HttpContext.Current;
            var db = context?.Items;
            if (db == null) { return func.Invoke(); }

            T ret = null;

            if (db.Contains(key))
            {
                ret = db[key] as T;
                if (ret != null)
                {
                    return ret;
                }
            }
            ret = func.Invoke();
            if (ret == null) { throw new Exception("不能缓存null对象"); }
            db[key] = ret;
            return ret;
        }

        /// <summary>
        /// 获取绝对路径
        /// </summary>
        /// <param name="context"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetMapPath(HttpContext context, string path)
        {
            try
            {
                return context.Server.MapPath(path);
            }
            catch
            {
                return MapPath(path);
            }
        }
        /// <summary>
        /// 获取绝对路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetMapPath(string path)
        {
            return GetMapPath(HttpContext.Current, path);
        }
        /// <summary>
        /// 来自nopcommerce的方法Maps a virtual path to a physical disk path.
        /// </summary>
        /// <param name="path">The path to map. E.g. "~/bin"</param>
        /// <returns>The physical path. E.g. "c:\inetpub\wwwroot\bin"</returns>
        private static string MapPath(string path)
        {
            if (HostingEnvironment.IsHosted)
            {
                //hosted
                return HostingEnvironment.MapPath(path);
            }
            //not hosted. For example, run in unit tests
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            path = path.Replace("~/", "").TrimStart('/').Replace('/', '\\');
            return Path.Combine(baseDirectory, path);
        }
        /// <summary>
        /// 获得当前应用程序的信任级别
        /// </summary>
        /// <returns></returns>
        public static AspNetHostingPermissionLevel GetTrustLevel()
        {
            AspNetHostingPermissionLevel trustLevel = AspNetHostingPermissionLevel.None;
            //权限列表
            AspNetHostingPermissionLevel[] levelList = new AspNetHostingPermissionLevel[] {
                                                                                            AspNetHostingPermissionLevel.Unrestricted,
                                                                                            AspNetHostingPermissionLevel.High,
                                                                                            AspNetHostingPermissionLevel.Medium,
                                                                                            AspNetHostingPermissionLevel.Low,
                                                                                            AspNetHostingPermissionLevel.Minimal
                                                                                            };

            foreach (AspNetHostingPermissionLevel level in levelList)
            {
                try
                {
                    //通过执行Demand方法检测是否抛出SecurityException异常来设置当前应用程序的信任级别
                    new AspNetHostingPermission(level).Demand();
                    trustLevel = level;
                    break;
                }
                catch (SecurityException ex)
                {
                    continue;
                }
            }
            return trustLevel;
        }


        /// <summary>
        /// 重启应用程序
        /// </summary>
        public static void RestartAppDomain(HttpContext context)
        {
            if (GetTrustLevel() > AspNetHostingPermissionLevel.Medium)//如果当前信任级别大于Medium，则通过卸载应用程序域的方式重启
            {
                HttpRuntime.UnloadAppDomain();
            }
            else//通过修改web.config方式重启应用程序
            {
                try
                {
                    File.SetLastWriteTime(ServerHelper.GetMapPath(context, "~/Web.config"), DateTime.Now);
                }
                catch { }
            }
        }
    }
}
