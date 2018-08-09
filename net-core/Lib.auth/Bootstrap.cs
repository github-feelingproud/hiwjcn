using Lib.auth.provider;
using Lib.infrastructure.entity.auth;
using Microsoft.Extensions.DependencyInjection;

namespace Lib.auth
{
    public static class AuthBootstrap
    {
        /// <summary>
        /// 配置auth
        /// </summary>
        /// <typeparam name="TokenBase"></typeparam>
        /// <typeparam name="CacheKeyManager"></typeparam>
        /// <typeparam name="UserLoginApi"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static IServiceCollection AuthConfig<TokenBase, CacheKeyManager, UserLoginApi>(this IServiceCollection collection)
            where TokenBase : AuthTokenBase, new()
            where CacheKeyManager : class, ICacheKeyManager
            where UserLoginApi : class, IUserLoginApi
        {
            collection.AddScoped<IScopedUserContext, ScopedUserContext>();
            collection.AddScoped<IAuthDataProvider, AppOrWebAuthDataProvider>();
            collection.AddScoped<IAuthApi, AuthApiServiceFromDbBase<TokenBase>>();
            collection.AddScoped<ITokenEncryption, DefaultTokenEncryption>();
            collection.AddScoped<ICacheKeyManager, CacheKeyManager>();
            collection.AddScoped<IUserLoginApi, UserLoginApi>();

            return collection;
        }
    }
}
