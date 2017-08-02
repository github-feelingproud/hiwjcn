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
    public interface IValidationDataProvider
    {
        string GetToken(HttpContext context);

        string GetClientID(HttpContext context);

        string GetClientSecurity(HttpContext context);
    }

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

    public class WebValidationDataProvider : IValidationDataProvider
    {
        private readonly LoginStatus _LoginStatus;

        public WebValidationDataProvider(LoginStatus _LoginStatus)
        {
            this._LoginStatus = _LoginStatus;
        }

        public string GetClientID(HttpContext context)
        {
            return ConfigurationManager.AppSettings["auth.client_id"]?.ToMD5();
        }

        public string GetClientSecurity(HttpContext context)
        {
            return ConfigurationManager.AppSettings["auth.client_security"]?.ToMD5();
        }

        public string GetToken(HttpContext context)
        {
            return this._LoginStatus.GetLoginUser()?.LoginToken;
        }
    }
}
