using Lib.extension;
using Lib.ioc;
using Lib.mvc.user;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Lib.mvc.auth.validation
{
    public interface ITokenValidationProvider
    {
        Task<LoginUserInfo> GetLoginUserInfoAsync(HttpContext context);
    }

    /// <summary>
    /// 拿到token和client信息后去验证信息，并拿到用户信息
    /// </summary>
    public abstract class TokenValidationProviderBase : ITokenValidationProvider
    {
        /// <summary>
        /// 缓存一个请求中的登录用户信息
        /// </summary>
        public abstract string HttpContextItemKey();

        public abstract LoginUserInfo FindUser(HttpContext context);

        public abstract Task<LoginUserInfo> FindUserAsync(HttpContext context);

        public virtual void WhenUserNotLogin(HttpContext context)
        {
            using (var s = IocContext.Instance.Scope())
            {
                var refresh = (s.ResolveConfig_()["refresh_login_status"] ?? "false").ToBool();
                if (refresh)
                    s.ResolveOptional_<LoginStatus>()?.SetUserLogout(context);
            }
        }

        public virtual void WhenUserLogin(HttpContext context, LoginUserInfo loginuser)
        {
            using (var s = IocContext.Instance.Scope())
            {
                var refresh = (s.ResolveConfig_()["refresh_login_status"] ?? "false").ToBool();
                if (refresh)
                    s.ResolveOptional_<LoginStatus>()?.SetUserLogin(context, loginuser);
            }
        }

        async Task<LoginUserInfo> ITokenValidationProvider.GetLoginUserInfoAsync(HttpContext context)
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
}
