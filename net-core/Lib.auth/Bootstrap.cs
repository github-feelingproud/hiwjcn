using Lib.mvc.auth.validation;
using Lib.mvc.user;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Lib.auth
{
    public static class AuthBootstrap
    {
        /// <summary>
        /// 配置auth
        /// </summary>
        public static void AuthBasicConfig<AuthApiProvider>(this IServiceCollection collection,
            Func<LoginStatus> cookieProvider = null)
            where AuthApiProvider : class, IAuthApi
        {
            collection.AuthConfig<AppOrWebAuthDataProvider, AuthApiProvider>(cookieProvider);
        }

        /// <summary>
        /// 配置auth
        /// </summary>
        public static void AuthConfig<TokenProvider, AuthApiProvider>(this IServiceCollection collection,
            Func<LoginStatus> cookieProvider = null)
            where TokenProvider : class, IAuthDataProvider
            where AuthApiProvider : class, IAuthApi
        {
            //从那里拿token和client信息
            collection.AddTransient<IAuthDataProvider, TokenProvider>();
            //怎么创建token scope等
            collection.AddTransient<IAuthApi, AuthApiProvider>();

            //使用以上信息
            collection.AddTransient<ITokenValidationProvider, AuthBasicValidationProvider>();

            //往那里写cookie
            if (cookieProvider != null)
            {
                collection.AddSingleton<LoginStatus>(_ => cookieProvider.Invoke());
            }
        }
    }
}
