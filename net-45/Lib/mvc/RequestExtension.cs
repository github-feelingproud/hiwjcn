using Lib.extension;
using Lib.helper;
using Lib.net;
using System;
using System.Linq;
using System.Web;

namespace Lib.mvc
{
    public static class RequestExtension
    {
        #region 获取get，post数据
        /// <summary>
        /// Get
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetGetString(this HttpRequest Request, string key, string deft = "")
        {
            return Request.QueryString[key] ?? deft;
        }
        /// <summary>
        /// Post
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetPostString(this HttpRequest Request, string key, string deft = "")
        {
            return Request.Form[key] ?? deft;
        }
        /// <summary>
        /// Request
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetParamsString(this HttpRequest Request, string key, string deft = "")
        {
            return Request.Params[key] ?? deft;
        }
        #endregion
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string GetBaseUrl(this HttpRequest request)
        {
            return request.Url.Scheme + "://" + request.Url.Authority + request.ApplicationPath;
        }
        public static string GetCurrentUrl(this HttpRequest request)
        {
            //string url = GetBaseUrl(request)+request.Url.Scheme + request.Path +"/"+ request.QueryString;
            //Debug.Write(url);
            return request.Url.ToString();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string MapPath(this HttpRequest Request, string path)
        {
            return ConvertHelper.GetString(Request.MapPath(path));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Request"></param>
        /// <returns></returns>
        public static bool IsPost(this HttpRequest Request)
        {
            return Request.HttpMethod?.ToUpper() == "POST";
        }
        public static bool IsGet(this HttpRequest Request)
        {
            return Request.HttpMethod?.ToUpper() == "GET";
        }
        public static bool IsAjax(this HttpRequest Request)
        {
            return Request.Headers["X-Requested-With"]?.ToUpper() == "XMLHttpRequest";
        }
        public static bool IsSSL(this HttpRequest Request)
        {
            return Request.IsSecureConnection;
        }

        /// <summary>
        /// 获得上次请求的url
        /// </summary>
        /// <returns></returns>
        public static string GetUrlReferrer(this HttpRequest Request)
        {
            //string re=HttpContext.Current.Request.Headers["referrer"];
            var uri = Request.UrlReferrer;
            return ConvertHelper.GetString(uri);
        }

        /// <summary>
        /// 获得请求的原始url
        /// </summary>
        /// <returns></returns>
        public static string GetRawUrl(this HttpRequest Request)
        {
            return ConvertHelper.GetString(Request.RawUrl);
        }

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
            var xff = Request.Headers.AllKeys
                .Where(x => "X-FORWARDED-FOR".Equals(x, StringComparison.InvariantCultureIgnoreCase))
                .Select(k => Request.Headers[k])
                .FirstOrDefault();

            //if you want to exclude private IP addresses, then see http://stackoverflow.com/questions/2577496/how-can-i-get-the-clients-ip-address-in-asp-net-mvc
            if (ValidateHelper.IsPlumpString(xff))
            {
                string lastIp = xff.Split(new[] { ',' }).FirstOrDefault();
                result = lastIp;
            }

            if (!ValidateHelper.IsPlumpString(result) && Request.UserHostAddress != null)
            {
                result = Request.UserHostAddress;
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

        public static string[] GetUserLanguages(this HttpRequest Request)
        {
            return Request.UserLanguages;
        }
        public static string GetUserAgent(this HttpRequest Request)
        {
            return ConvertHelper.GetString(Request.UserAgent);
        }
        public static string GetUserHostName(this HttpRequest Request)
        {
            return ConvertHelper.GetString(Request.UserHostName);
        }
        public static string GetUserHost(this HttpRequest Request)
        {
            return ConvertHelper.GetString(Request.Url.Host);
        }

        public static bool IsSearchEnginesGet(this HttpRequest Request)
        {
            if (Request.UrlReferrer == null)
                return false;

            string[] SearchEngine = { "google", "yahoo", "msn", "baidu", "sogou", "sohu", "sina", "163", "lycos", "tom", "yisou", "iask", "soso", "gougou", "zhongsou" };
            string tmpReferrer = Request.UrlReferrer.ToString().ToLower();
            return SearchEngine.Any(x => tmpReferrer.Contains(x));
        }

        #region 客户端信息

        /// <summary>
        /// 获得请求的浏览器类型
        /// </summary>
        /// <returns></returns>
        public static string GetBrowserType(this HttpRequest Request)
        {
            string type = Request.Browser.Type;
            if (string.IsNullOrEmpty(type) || type == "unknown")
                return "未知";

            return type.ToLower();
        }

        /// <summary>
        /// 获得请求的浏览器名称
        /// </summary>
        /// <returns></returns>
        public static string GetBrowserName(this HttpRequest Request)
        {
            string name = Request.Browser.Browser;
            if (string.IsNullOrEmpty(name) || name == "unknown")
                return "未知";

            return name.ToLower();
        }

        /// <summary>
        /// 获得请求的浏览器版本
        /// </summary>
        /// <returns></returns>
        public static string GetBrowserVersion(this HttpRequest Request)
        {
            string version = Request.Browser.Version;
            if (string.IsNullOrEmpty(version) || version == "unknown")
                return "未知";

            return version;
        }

        /// <summary>
        /// 获得请求客户端的操作系统类型
        /// </summary>
        /// <returns></returns>
        public static string GetOSType(this HttpRequest Request)
        {
            string type = "未知";
            string userAgent = Request.UserAgent;

            if (userAgent.Contains("NT 6.1"))
            {
                type = "Windows 7";
            }
            else if (userAgent.Contains("NT 5.1"))
            {
                type = "Windows XP";
            }
            else if (userAgent.Contains("NT 6.2"))
            {
                type = "Windows 8";
            }
            else if (userAgent.Contains("android"))
            {
                type = "Android";
            }
            else if (userAgent.Contains("iphone"))
            {
                type = "IPhone";
            }
            else if (userAgent.Contains("Mac"))
            {
                type = "Mac";
            }
            else if (userAgent.Contains("NT 6.0"))
            {
                type = "Windows Vista";
            }
            else if (userAgent.Contains("NT 5.2"))
            {
                type = "Windows 2003";
            }
            else if (userAgent.Contains("NT 5.0"))
            {
                type = "Windows 2000";
            }
            else if (userAgent.Contains("98"))
            {
                type = "Windows 98";
            }
            else if (userAgent.Contains("95"))
            {
                type = "Windows 95";
            }
            else if (userAgent.Contains("Me"))
            {
                type = "Windows Me";
            }
            else if (userAgent.Contains("NT 4"))
            {
                type = "Windows NT4";
            }
            else if (userAgent.Contains("Unix"))
            {
                type = "UNIX";
            }
            else if (userAgent.Contains("Linux"))
            {
                type = "Linux";
            }
            else if (userAgent.Contains("SunOS"))
            {
                type = "SunOS";
            }

            return type;
        }

        /// <summary>
        /// 获得请求客户端的操作系统名称
        /// </summary>
        /// <returns></returns>
        public static string GetOSName(this HttpRequest Request)
        {
            string name = Request.Browser.Platform;
            if (string.IsNullOrEmpty(name))
                return "未知";

            return name;
        }

        /// <summary>
        /// 判断是否是浏览器请求
        /// </summary>
        /// <returns></returns>
        public static bool IsBrowser(this HttpRequest Request)
        {
            var list = new string[] { "ie", "chrome", "mozilla", "netscape", "firefox", "opera" };
            string name = GetBrowserName(Request);
            return list.Any(x => name.Contains(x));
        }

        /// <summary>
        /// 是否是移动设备请求
        /// </summary>
        /// <returns></returns>
        public static bool IsMobile(HttpContext context)
        {
            if (context.Request.Browser.IsMobileDevice) { return true; }
            var IsTablet = ConvertHelper.GetString(context.Request.Browser["IsTablet"]).ToLower();
            //return IsTablet == "true" || IsTablet == "1";
            return IsTablet.ToBool();
        }

        #endregion
    }
}
