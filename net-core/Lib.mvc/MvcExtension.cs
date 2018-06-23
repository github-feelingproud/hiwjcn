using Lib.extension;
using Lib.helper;
using Lib.io;
using Lib.mvc.user;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Lib.mvc
{
    public static class MvcExtension
    {
        /// <summary>
        /// 获取Area Controller Action
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        public static (string area, string controller, string action) GetRouteInfo(this RouteData route)
        {
            var AreaName = ConvertHelper.GetString(route.Values["Area"]);
            var ControllerName = ConvertHelper.GetString(route.Values["Controller"]);
            var ActionName = ConvertHelper.GetString(route.Values["Action"]);
            return (AreaName, ControllerName, ActionName);
        }

        /// <summary>
        /// 设置请求ID
        /// </summary>
        public static void SetNewRequestID(this HttpContext context) => context.Items["req_guid"] = Com.GetUUID();

        /// <summary>
        /// 获取请求ID
        /// </summary>
        /// <returns></returns>
        public static string GetRequestID(this HttpContext context) => ConvertHelper.GetString(context.Items["req_guid"]);

        /// <summary>
        /// 获取类似/home/index的url
        /// </summary>
        public static string ActionUrl(this RouteData route)
        {
            var data = route.GetRouteInfo();
            var sp = new string[] { data.area, data.controller, data.action }.Where(x => ValidateHelper.IsPlumpString(x)).ToList();
            if (!ValidateHelper.IsPlumpList(sp))
            {
                throw new Exception("无法获取action访问路径");
            }
            return "/" + "/".Join_(sp);
        }

        /// <summary>
        /// 获取上传文件的字节数组
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static byte[] GetBytes(this IFormFile file)
        {
            using (var s = file.OpenReadStream())
            {
                s.Seek(0, SeekOrigin.Begin);
                var bs = s.GetBytes();
                return bs;
            }
        }

        /// <summary>
        /// post和get数据的合并
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Dictionary<string, string> PostAndGet(this HttpContext context) =>
            context.QueryStringToDict().AddDict(context.PostToDict());

        /*
            var dict = context.Request.Form.ToDict();
            dict = dict.AddDict(context.Request.QueryString.ToDict());
            return dict;
             */

        /// <summary>
        /// get数据
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Dictionary<string, string> QueryStringToDict(this HttpContext context) =>
            context.Request.Query.ToDict();

        /// <summary>
        /// post数据
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Dictionary<string, string> PostToDict(this HttpContext context) =>
            context.Request.Form.ToDict();

        public static Dictionary<string, string> ToDict(this IEnumerable<KeyValuePair<string, StringValues>> data)
            => data.ToDictionary(x => x.Key, x => (string)x.Value);

        /// <summary>
        /// 获取这个程序集中所用到的所有权限
        /// </summary>\
        public static List<string> ScanAllAssignedPermissionOnThisAssembly(this Assembly ass)
        {
            var permission_list = new List<string>();
            var tps = ass.GetTypes();
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
                        permission_list.AddRange(pers);
                    }
                }
            }
            permission_list = permission_list.Distinct().Where(x => ValidateHelper.IsPlumpString(x)).ToList();

            return permission_list;
        }
    }
}
