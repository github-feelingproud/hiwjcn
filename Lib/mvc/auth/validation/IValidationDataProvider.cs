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
using Lib.helper;

namespace Lib.mvc.auth.validation
{
    public static class WebClientConfig
    {
        private static readonly Lazy<string> id = new Lazy<string>(() => ConfigurationManager.AppSettings["auth.client_id"]?.ToMD5()?.ToLower());
        private static readonly Lazy<string> security = new Lazy<string>(() => ConfigurationManager.AppSettings["auth.client_security"]?.ToMD5()?.ToLower());

        public static string ClientID() => id.Value;
        public static string ClientSecurity() => security.Value;
    }

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

        public string GetClientID(HttpContext context) => WebClientConfig.ClientID();

        public string GetClientSecurity(HttpContext context) => WebClientConfig.ClientSecurity();

        public string GetToken(HttpContext context)
        {
            return this._LoginStatus.GetCookieToken();
        }
    }

    public class AppOrWebTokenProvider : IValidationDataProvider
    {
        public readonly LoginStatus _loginstatus;

        public AppOrWebTokenProvider(LoginStatus _loginstatus)
        {
            this._loginstatus = _loginstatus;
        }

        public string GetClientID(HttpContext context)
        {
            var client_id = context.GetAuthClientID();
            if (!ValidateHelper.IsPlumpString(client_id))
            {
                client_id = WebClientConfig.ClientID();
            }
            return client_id;
        }

        public string GetClientSecurity(HttpContext context)
        {
            var client_security = context.GetAuthClientSecurity();
            if (!ValidateHelper.IsPlumpString(client_security))
            {
                client_security = WebClientConfig.ClientSecurity();
            }
            return client_security;
        }

        public string GetToken(HttpContext context)
        {
            var token = context.GetAuthToken();
            if (!ValidateHelper.IsPlumpString(token))
            {
                token = this._loginstatus.GetCookieToken(context);
            }
            return token;
        }
    }
}
