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
    /// <summary>
    /// 尝试获取app或者web的token信息
    /// </summary>
    public class AppOrWebAuthDataProvider : IAuthDataProvider
    {
        public readonly LoginStatus _loginstatus;

        public AppOrWebAuthDataProvider(LoginStatus _loginstatus)
        {
            this._loginstatus = _loginstatus;
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
