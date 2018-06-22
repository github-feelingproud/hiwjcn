using Lib.helper;
using Lib.ioc;
using Lib.mvc.auth.validation;
using Lib.mvc.user;
using System;
using System.Threading.Tasks;
using System.Web;

namespace Lib.mvc.auth
{
    public static class AuthExtension
    {
        /// <summary>
        /// 获取当前登录用户
        /// </summary>
        public static async Task<LoginUserInfo> GetAuthUserAsync(this HttpContext context, string name = null)
        {
            using (var x = AutofacIocContext.Instance.Scope())
            {
                var loginuser = await x.Resolve_<ITokenValidationProvider>(name).GetLoginUserInfoAsync(context);

                return loginuser;
            }
        }

        /// <summary>
        /// 获取当前登录用户
        /// </summary>
        public static LoginUserInfo GetAuthUser(this HttpContext context, string name = null)
        {
            using (var x = AutofacIocContext.Instance.Scope())
            {
                var loginuser = x.Resolve_<ITokenValidationProvider>(name).GetLoginUserInfo(context);

                return loginuser;
            }
        }

        /// <summary>
        /// 获取bearer token
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetBearerToken(this HttpContext context)
        {
            var bearer = "Bearer" + ' '.ToString();
            var token = context.Request.Headers["Authorization"] ?? string.Empty;
            if (token.StartsWith(bearer, StringComparison.OrdinalIgnoreCase))
            {
                return token.Substring(bearer.Length);
            }
            return string.Empty;
        }

        /// <summary>
        /// 获取bearer token或者header.auth.token
        /// </summary>
        public static string GetAuthToken(this HttpContext context)
        {
            var tk = context.GetBearerToken();
            if (!ValidateHelper.IsPlumpString(tk))
            {
                tk = context.Request.Headers["auth.token"];
            }
            return tk;
        }

        /// <summary>
        /// 使用cookie登录
        /// </summary>
        public static void CookieLogin(this HttpContext context, LoginUserInfo loginuser)
        {
            using (var s = AutofacIocContext.Instance.Scope())
            {
                var loginstatus = s.Resolve_<LoginStatus>();
                loginstatus.SetUserLogin(context, loginuser);
            }
        }

        /// <summary>
        /// 退出登录
        /// </summary>
        public static void CookieLogout(this HttpContext context)
        {
            using (var s = AutofacIocContext.Instance.Scope())
            {
                var loginstatus = s.Resolve_<LoginStatus>();
                loginstatus.SetUserLogout(context);
            }
        }
    }
}
