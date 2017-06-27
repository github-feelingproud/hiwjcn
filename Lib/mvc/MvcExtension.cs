using Lib.helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Lib.extension;
using Lib.io;
using Lib.mvc.user;
using System.Reflection;
using System.Web.SessionState;

namespace Lib.mvc
{
    public static class MvcExtension
    {
        /// <summary>
        /// 获取Area Controller Action
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static (string area, string controller, string action) GetA_C_A(this RouteData data)
        {
            var AreaName = ConvertHelper.GetString(data.Values["Area"]);
            var ControllerName = ConvertHelper.GetString(data.Values["Controller"]);
            var ActionName = ConvertHelper.GetString(data.Values["Action"]);
            return (AreaName, ControllerName, ActionName);
        }

        /// <summary>
        /// 获取IP
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public static string Ip(this HttpRequest req)
        {
            return RequestHelper.GetCurrentIpAddress(req);
        }

        /// <summary>
        /// 是否是post
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public static bool IsPost(this HttpRequest req)
        {
            return RequestHelper.IsPost(req);
        }

        /// <summary>
        /// 是否是ajax
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public static bool IsAjax(this HttpRequest req)
        {
            return RequestHelper.IsAjax(req);
        }

        /// <summary>
        /// 获取根目录
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public static string GetBaseUrl(this HttpRequest req)
        {
            return RequestHelper.GetBaseUrl(req);
        }

        /// <summary>
        /// 获取当前访问地址
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public static string GetCurrentUrl(this HttpRequest req)
        {
            return RequestHelper.GetCurrentUrl(req);
        }

        /// <summary>
        /// 获取上传文件的字节数组
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static byte[] GetBytes(this HttpPostedFile file)
        {
            var bs = IOHelper.GetPostFileBytesAndDispose(file);
            return bs;
        }

        /// <summary>
        /// 是否是SSL
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public static bool IsSSL(this HttpRequest req)
        {
            return RequestHelper.IsSSL(req);
        }

        /// <summary>
        /// post和get数据的合并
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Dictionary<string, string> PostAndGet(this HttpContext context)
        {
            var dict = context.Request.Form.ToDict();
            dict = dict.AddDict(context.Request.QueryString.ToDict());
            return dict;
        }

        /// <summary>
        /// 设置实体
        /// </summary>
        /// <param name="session"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void SetObject(this HttpSessionState session, string key, object value)
        {
            session[key] = value.ToJson();
        }

        /// <summary>
        /// 获取实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T GetObject<T>(this HttpSessionState session, string key)
        {
            var value = session[key]?.ToString();

            return value == null ? default(T) : value.JsonToEntity<T>();
        }

        /// <summary>
        /// 获取这个程序集中所用到的所有权限
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        public static List<string> ScanAllAssignedPermissionOnThisAssembly(this Controller controller)
        {
            var list = new List<string>();
            var tps = controller.GetType().Assembly.GetTypes();
            tps = tps.Where(x => x.IsNormalClass() && x.IsAssignableTo_<Controller>()).ToArray();
            foreach (var t in tps)
            {
                var methods = t.GetMethods().Where(x => x.IsPublic);
                foreach (var m in methods)
                {
                    var attr = m.GetCustomAttribute<ValidLoginBaseAttribute>();
                    if (attr == null) { continue; }
                    var pers = attr.Permission?.Split(',').ToList();
                    if (ValidateHelper.IsPlumpList(pers))
                    {
                        list.AddRange(pers);
                    }
                }
            }
            return list.Distinct().Where(x => ValidateHelper.IsPlumpString(x)).ToList();
        }
    }
}
