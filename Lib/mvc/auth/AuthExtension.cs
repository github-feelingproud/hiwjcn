using Lib.helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Lib.extension;
using Lib.io;
using Lib.ioc;
using Lib.mvc.user;
using System.Reflection;
using System.Web.SessionState;
using Autofac;
using Lib.mvc.auth.validation;

namespace Lib.mvc.auth
{
    public static class AuthExtension
    {
        public const string AuthedUserKey = "context.items.auth.user.entity";

        /// <summary>
        /// 获取当前登录用户
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static async Task<LoginUserInfo> GetAuthUserAsync(this HttpContext context)
        {
            var data = await context.CacheInHttpContextAsync(AuthedUserKey, async () =>
            {
                return await AppContext.ScopeAsync(async x =>
                {
                    var loginuser = await x.Resolve_<TokenValidationProviderBase>().FindUserAsync(context);
                    if (loginuser == null)
                    {
                        var loginstatus = x.ResolveOptional<LoginStatus>();
                        loginstatus?.SetUserLogout(context);
                    }
                    return loginuser;
                });
            });
            return data;
        }

        /// <summary>
        /// 获取当前登录用户
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static LoginUserInfo GetAuthUser(this HttpContext context)
        {
            var data = context.CacheInHttpContext(AuthedUserKey, () =>
            {
                return AppContext.Scope(x =>
                {
                    var loginuser = x.Resolve_<TokenValidationProviderBase>().FindUser(context);
                    if (loginuser == null)
                    {
                        var loginstatus = x.ResolveOptional<LoginStatus>();
                        loginstatus?.SetUserLogout(context);
                    }
                    return loginuser;
                });
            });
            return data;
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
        /// 获取client id
        /// </summary>
        public static string GetAuthClientID(this HttpContext context) =>
            context.Request.Headers["auth.client_id"] ?? string.Empty;

        /// <summary>
        /// 获取client security
        /// </summary>
        public static string GetAuthClientSecurity(this HttpContext context) =>
            context.Request.Headers["auth.client_security"] ?? string.Empty;

        /// <summary>
        /// 使用cookie登录
        /// </summary>
        public static void CookieLogin(this HttpContext context, LoginUserInfo loginuser)
        {
            AppContext.Scope(s =>
            {
                var loginstatus = s.Resolve_<LoginStatus>();
                loginstatus.SetUserLogin(context, loginuser);
                return true;
            });
        }

        /// <summary>
        /// 退出登录
        /// </summary>
        public static void CookieLogout(this HttpContext context)
        {
            AppContext.Scope(s =>
            {
                var loginstatus = s.Resolve_<LoginStatus>();
                loginstatus.SetUserLogout(context);
                return true;
            });
        }

        /// <summary>
        /// cookie store
        /// </summary>
        public static void AuthUseLoginStatus(this ContainerBuilder builder, Func<LoginStatus> config)
        {
            builder.Register(_ => config.Invoke()).AsSelf().AsImplementedInterfaces().SingleInstance();
        }

        /// <summary>
        /// 获取token client的逻辑
        /// </summary>
        public static void AuthUseValidationDataProvider<T>(this ContainerBuilder builder)
            where T : IValidationDataProvider
        {
            builder.RegisterType<T>().AsSelf().AsImplementedInterfaces().SingleInstance();
        }

        /// <summary>
        /// auth服务器配置
        /// </summary>
        public static void AuthUseServerConfig<T>(this ContainerBuilder builder)
            where T : AuthServerConfig
        {
            builder.RegisterType<T>().AsSelf().AsImplementedInterfaces().SingleInstance();
        }

        /// <summary>
        /// 自定义验证
        /// </summary>
        public static void AuthClientUseCustomValidation<T>(this ContainerBuilder builder)
            where T : TokenValidationProviderBase
        {
            builder.RegisterType<T>().AsSelf().As<TokenValidationProviderBase>().SingleInstance();
        }
    }
}
