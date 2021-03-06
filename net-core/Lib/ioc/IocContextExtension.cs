﻿using Lib.cache;
using Lib.net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data;
using System.Linq;

namespace Lib.ioc
{
    public static class IocContextExtension
    {
        /// <summary>
        /// 作为顶级容器
        /// </summary>
        /// <param name="s"></param>
        public static void SetAsRootIServiceProvider(this IServiceProvider s) =>
            IocContext.Instance.SetRootContainer(s);

        public static T Resolve_<T>(this IServiceProvider provider) =>
            provider.GetRequiredService<T>();

        public static T ResolveOptional_<T>(this IServiceProvider provider) =>
            provider.GetService<T>();

        public static T[] ResolveAll_<T>(this IServiceProvider provider) =>
            provider.GetServices<T>().ToArray();

        public static IConfiguration ResolveConfig_(this IServiceProvider provider) =>
            provider.Resolve_<IConfiguration>();

        //-------------------------------------------------------------------------

        public static T Resolve_<T>(this IServiceScope scope) =>
            scope.ServiceProvider.Resolve_<T>();
        
        public static T ResolveOptional_<T>(this IServiceScope scope) =>
            scope.ServiceProvider.ResolveOptional_<T>();
        
        public static T[] ResolveAll_<T>(this IServiceScope scope) =>
            scope.ServiceProvider.ResolveAll_<T>();
        
        public static IConfiguration ResolveConfig_(this IServiceScope scope) =>
            scope.ServiceProvider.ResolveConfig_();

        //-----------------------------------------------------------------------------

        public static void AddComponentDisposer<T>(this IServiceCollection collection)
            where T : class, IDisposeComponent
            => collection.AddSingleton<IDisposeComponent, T>();

        public static void UseLib(this IServiceCollection collection)
        {
            collection.AddSingleton(x => HttpClientManager.Instance.DefaultClient);

            collection.AddComponentDisposer<LibCoreDisposeComponent>();
        }
    }

    public static class IocExtension____
    {
        /// <summary>
        /// 使用数据库
        /// </summary>
        public static IServiceCollection UseAdoConnection(this IServiceCollection collection, Func<IDbConnection> get_opened_connction) =>
            collection.AddTransient<IDbConnection>(_ => get_opened_connction.Invoke());

        /// <summary>
        /// 使用缓存
        /// </summary>
        public static IServiceCollection UseCacheProvider<T>(this IServiceCollection collection)
            where T : class, ICacheProvider =>
            collection.AddSingleton<ICacheProvider, T>();
    }
}
