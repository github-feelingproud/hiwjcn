using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Lib.ioc;
using Lib.helper;
using Lib.core;
using Lib.extension;
using Lib.mvc.user;

namespace Lib.mvc.auth.validation
{
    public abstract class TokenValidationProviderBase
    {
        public abstract LoginUserInfo FindUser(HttpContext context);

        public virtual async Task<LoginUserInfo> FindUserAsync(HttpContext context) => await Task.FromResult(this.FindUser(context));
    }

    /// <summary>
    /// 直接使用login status
    /// </summary>
    public class CookieTokenValidationProvider : TokenValidationProviderBase
    {
        private readonly LoginStatus _LoginStatus;

        public CookieTokenValidationProvider(LoginStatus _LoginStatus)
        {
            this._LoginStatus = _LoginStatus;
        }

        public override LoginUserInfo FindUser(HttpContext context)
        {
            return this._LoginStatus.GetLoginUser(context);
        }
    }

    /// <summary>
    /// 请求auth server验证
    /// </summary>
    public class AuthServerValidationProvider : TokenValidationProviderBase
    {
        public override LoginUserInfo FindUser(HttpContext context)
        {
            throw new NotImplementedException();
        }

        public override async Task<LoginUserInfo> FindUserAsync(HttpContext context)
        {
            await Task.FromResult(1);
            throw new NotImplementedException();
        }
    }
}
