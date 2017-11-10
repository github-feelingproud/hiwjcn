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
using System.Net;
using System.Net.Http;
using Lib.net;

namespace Lib.mvc.auth.validation
{
    public interface ITokenValidationProvider
    {
        LoginUserInfo GetLoginUserInfo(HttpContext context);

        Task<LoginUserInfo> GetLoginUserInfoAsync(HttpContext context);
    }

    /// <summary>
    /// 拿到token和client信息后去验证信息，并拿到用户信息
    /// </summary>
    public abstract class TokenValidationProviderBase : ITokenValidationProvider
    {
        public virtual string HttpContextItemKey() => "context.items.auth.user.entity";
        
        public abstract LoginUserInfo FindUser(HttpContext context);
        
        public abstract Task<LoginUserInfo> FindUserAsync(HttpContext context);

        public virtual void WhenUserNotLogin(HttpContext context)
        {
            AppContext.Scope(s =>
            {
                s.ResolveOptional_<LoginStatus>()?.SetUserLogout(context);
                return true;
            });
        }

        public virtual void WhenUserLogin(HttpContext context, LoginUserInfo loginuser)
        {
            AppContext.Scope(s =>
            {
                s.ResolveOptional_<LoginStatus>()?.SetUserLogin(context, loginuser);
                return true;
            });
        }

        public LoginUserInfo GetLoginUserInfo(HttpContext context)
        {
            var data = context.CacheInHttpContext(this.HttpContextItemKey(), () =>
            {
                var loginuser = this.FindUser(context);
                if (loginuser == null)
                {
                    this.WhenUserNotLogin(context);
                }
                else
                {
                    this.WhenUserLogin(context, loginuser);
                }

                return loginuser;
            });
            return data;
        }

        public async Task<LoginUserInfo> GetLoginUserInfoAsync(HttpContext context)
        {
            var data = await context.CacheInHttpContextAsync(this.HttpContextItemKey(), async () =>
            {
                var loginuser = await this.FindUserAsync(context);
                if (loginuser == null)
                {
                    this.WhenUserNotLogin(context);
                }
                else
                {
                    this.WhenUserLogin(context, loginuser);
                }

                return loginuser;
            });
            return data;
        }

    }

    /// <summary>
    /// 使用了auth api来验证
    /// </summary>
    public class AuthBasicValidationProvider : TokenValidationProviderBase
    {
        private readonly IAuthDataProvider _dataProvider;
        private readonly IAuthApi api;

        public AuthBasicValidationProvider(
            IAuthDataProvider _dataProvider,
            IAuthApi api)
        {
            this._dataProvider = _dataProvider;
            this.api = api;
        }

        public override LoginUserInfo FindUser(HttpContext context)
        {
            return AsyncHelper.RunSync(() => FindUserAsync(context));
        }

        public override async Task<LoginUserInfo> FindUserAsync(HttpContext context)
        {
            try
            {
                var access_token = this._dataProvider.GetToken(context);
                var client_id = this._dataProvider.GetClientID(context);

                var loginuser = await this.api.GetLoginUserInfoByTokenAsync(client_id, access_token);

                if (!loginuser.success)
                {
                    loginuser.msg?.AddBusinessInfoLog();
                    return null;
                }

                return loginuser.data;
            }
            catch (Exception e)
            {
                e.AddErrorLog();
                return null;
            }
        }
    }
}
