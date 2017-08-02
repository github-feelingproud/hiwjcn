using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Lib.mvc.auth;
using Lib.mvc.user;
using System.Configuration;
using Lib.extension;

namespace Lib.mvc.auth.validation
{
    /// <summary>
    /// 获取token和client 信息的渠道
    /// </summary>
    public interface IValidationDataProvider
    {
        string GetToken(HttpContext context);

        string GetClientID(HttpContext context);

        string GetClientSecurity(HttpContext context);
    }

    /// <summary>
    /// 从header中获取token和client信息
    /// </summary>
    public class AppValidationDataProvider : IValidationDataProvider
    {
        public string GetClientID(HttpContext context)
        {
            return context.GetAuthClientID();
        }

        public string GetClientSecurity(HttpContext context)
        {
            return context.GetAuthClientSecurity();
        }

        public string GetToken(HttpContext context)
        {
            return context.GetAuthToken();
        }
    }

    /// <summary>
    /// 从cookie中获取token，从web.config中获取client信息
    /// </summary>
    public class WebValidationDataProvider : IValidationDataProvider
    {
        private readonly LoginStatus _LoginStatus;

        public WebValidationDataProvider(LoginStatus _LoginStatus)
        {
            this._LoginStatus = _LoginStatus;
        }

        public string GetClientID(HttpContext context)
        {
            var data = ConfigurationManager.AppSettings["auth.client_id"];
            return data?.ToMD5();
        }

        public string GetClientSecurity(HttpContext context)
        {
            var data = ConfigurationManager.AppSettings["auth.client_security"];
            return data?.ToMD5();
        }

        public string GetToken(HttpContext context)
        {
            return this._LoginStatus.GetCookieToken();
        }
    }
}
