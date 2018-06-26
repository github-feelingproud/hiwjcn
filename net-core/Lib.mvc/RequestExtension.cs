using Lib.helper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;

namespace Lib.mvc
{
    public static class RequestExtension
    {
        [Obsolete]
        public static string GetBaseUrl(this HttpRequest request) => request.PathBase;
        [Obsolete]
        public static string GetCurrentUrl(this HttpRequest request) => request.Path;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Request"></param>
        /// <returns></returns>
        public static bool IsPost(this HttpRequest Request) =>
            Request.Method?.ToUpper() == "POST";

        public static bool IsGet(this HttpRequest Request) =>
            Request.Method?.ToUpper() == "GET";

        public static bool IsAjax(this HttpRequest Request)
        {
            StringValues? data = Request.Headers["X-Requested-With"];
            return ((string)(data ?? StringValues.Empty)).ToUpper() == "XMLHttpRequest";
        }
        public static bool IsSSL(this HttpRequest Request) => throw new NotImplementedException();

        /// <summary>
        /// 获得上次请求的url
        /// </summary>
        /// <returns></returns>
        public static string GetUrlReferrer(this HttpRequest Request) => throw new NotImplementedException();

        /// <summary>
        /// 获取ip地址，方法来自nopcommerce。计算方式比较多
        /// </summary>
        /// <param name="Request"></param>
        /// <returns></returns>
        public static string GetCurrentIpAddress(this HttpRequest Request)
        {
            var result = string.Empty;
            //The X-Forwarded-For (XFF) HTTP header field is a de facto standard
            //for identifying the originating IP address of a client
            //connecting to a web server through an HTTP proxy or load balancer.

            //it's used for identifying the originating IP address of a client connecting to a web server
            //through an HTTP proxy or load balancer. 
            var xff = (string)Request.Headers.Keys
                .Where(x => "X-FORWARDED-FOR".Equals(x, StringComparison.InvariantCultureIgnoreCase))
                .Select(k => Request.Headers[k])
                .FirstOrDefault();

            //if you want to exclude private IP addresses, then see http://stackoverflow.com/questions/2577496/how-can-i-get-the-clients-ip-address-in-asp-net-mvc
            if (ValidateHelper.IsPlumpString(xff))
            {
                string lastIp = xff.Split(new[] { ',' }).FirstOrDefault();
                result = lastIp;
            }

            //some validation
            if (result == "::1")
            {
                result = "127.0.0.1";
            }
            //remove port
            if (ValidateHelper.IsPlumpString(result))
            {
                int index = result.IndexOf(":", StringComparison.InvariantCultureIgnoreCase);
                if (index > 0)
                {
                    result = result.Substring(0, index);
                }
            }
            return result;
        }
    }
}
