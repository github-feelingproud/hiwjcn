using Lib.helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Lib.mvc
{
    public static class MvcExtension
    {
        /// <summary>
        /// 获取Area Controller Action
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Tuple<string, string, string> GetA_C_A(this RouteData data)
        {
            var AreaName = ConvertHelper.GetString(data.Values["Area"]);
            var ControllerName = ConvertHelper.GetString(data.Values["Controller"]);
            var ActionName = ConvertHelper.GetString(data.Values["Action"]);
            return Tuple.Create(AreaName, ControllerName, ActionName);
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
        /// 是否是SSL
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public static bool IsSSL(this HttpRequest req)
        {
            return RequestHelper.IsSSL(req);
        }
    }
}
