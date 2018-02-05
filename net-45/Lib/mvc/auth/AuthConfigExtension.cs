using Autofac;
using Lib.mvc.auth.api;
using Lib.mvc.auth.validation;
using Lib.mvc.user;
using System;

namespace Lib.mvc.auth
{
    public static class AuthConfigExtension
    {
        /// <summary>
        /// 配置auth
        /// </summary>
        public static void AuthBasicServerConfig<LoginProvider>(this ContainerBuilder builder, Func<AuthServerConfig> serverProvider,
            Func<LoginStatus> cookieProvider = null)
            where LoginProvider : class, IAuthLoginProvider
        {
            builder.Register(_ => serverProvider.Invoke()).AsSelf().AsImplementedInterfaces().SingleInstance();
            builder.AuthConfig<LoginProvider, AppOrWebAuthDataProvider, AuthApiFromWcf>(cookieProvider);
        }

        /// <summary>
        /// 配置auth
        /// </summary>
        public static void AuthBasicConfig<LoginProvider, AuthApiProvider>(this ContainerBuilder builder,
            Func<LoginStatus> cookieProvider = null)
            where LoginProvider : class, IAuthLoginProvider
            where AuthApiProvider : class, IAuthApi
        {
            builder.AuthConfig<LoginProvider, AppOrWebAuthDataProvider, AuthApiProvider>(cookieProvider);
        }

        /// <summary>
        /// 配置auth
        /// </summary>
        public static void AuthConfig<LoginProvider, TokenProvider, AuthApiProvider>(this ContainerBuilder builder,
            Func<LoginStatus> cookieProvider = null)
            where LoginProvider : class, IAuthLoginProvider
            where TokenProvider : class, IAuthDataProvider
            where AuthApiProvider : class, IAuthApi
        {
            //登录逻辑
            builder.RegisterType<LoginProvider>().AsSelf().As<IAuthLoginProvider>();
            //从那里拿token和client信息
            builder.RegisterType<TokenProvider>().AsSelf().As<IAuthDataProvider>();
            //怎么创建token scope等
            builder.RegisterType<AuthApiProvider>().AsSelf().As<IAuthApi>();

            //使用以上信息
            builder.RegisterType<AuthBasicValidationProvider>().AsSelf().As<ITokenValidationProvider>();

            //往那里写cookie
            if (cookieProvider != null)
            {
                builder.Register(_ => cookieProvider.Invoke()).AsSelf().AsImplementedInterfaces().SingleInstance();
            }
        }
    }
}
