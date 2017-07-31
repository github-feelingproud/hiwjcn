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
        public const string AuthedUserKey = "auth.user.entity";

        /// <summary>
        /// 获取auth上下文
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static AuthContext AuthContext(this HttpContext context)
        {
            if (!AppContext.IsRegistered<ITokenProvider>())
            {
                throw new Exception("请注册token provider");
            }
            return new AuthContext(context, null);
        }

        /// <summary>
        /// 获取当前登录用户
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static async Task<LoginUserInfo> GetAuthUserAsync(this HttpContext context)
        {
            var data = await context.CacheInHttpContextAsync(AuthedUserKey, async () =>
            {
                if (!AppContext.IsRegistered<TokenValidationProviderBase>())
                {
                    throw new Exception($"没有注册{nameof(TokenValidationProviderBase)}");
                }

                return await AppContext.ScopeAsync(async x =>
                {
                    return await x.Resolve<TokenValidationProviderBase>().FindUserAsync(context);
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
                if (!AppContext.IsRegistered<TokenValidationProviderBase>())
                {
                    throw new Exception($"没有注册{nameof(TokenValidationProviderBase)}");
                }

                return AppContext.Scope(x =>
                {
                    return x.Resolve<TokenValidationProviderBase>().FindUser(context);
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
        /// 注册登录逻辑
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="config"></param>
        public static void AuthUseUserLoginService(this ContainerBuilder builder, Func<IAuthLoginService> config)
        {
            builder.Register(_ => config.Invoke()).AsSelf().AsImplementedInterfaces().SingleInstance();
        }

        /// <summary>
        /// 使用auth server验证
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="config"></param>
        public static void AuthUseAuthServerValidation(this ContainerBuilder builder, Func<AuthServerConfig> config)
        {
            builder.Register(_ => config.Invoke()).AsSelf().SingleInstance();
            builder.RegisterType<AuthServerValidationProvider>().AsSelf().As<TokenValidationProviderBase>().SingleInstance();
        }

        /// <summary>
        /// 使用token验证
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="config"></param>
        public static void AuthUseCookieValidation(this ContainerBuilder builder, Func<LoginStatus> config)
        {
            builder.Register(_ => config.Invoke()).AsSelf().SingleInstance();
            builder.RegisterType<CookieTokenValidationProvider>().AsSelf().As<TokenValidationProviderBase>().SingleInstance();
        }

        /// <summary>
        /// 自定义验证
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="config"></param>
        public static void AuthUseCustomValidation(this ContainerBuilder builder, Func<TokenValidationProviderBase> config)
        {
            builder.Register(_ => config.Invoke()).AsSelf().As<TokenValidationProviderBase>().SingleInstance();
        }
    }
}
