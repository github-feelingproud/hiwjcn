using Lib.core;
using Lib.net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
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

        public static T Resolve_<T>(this IServiceScope scope) =>
            scope.ServiceProvider.GetRequiredService<T>();

        public static T ResolveOptional_<T>(this IServiceScope scope) =>
            scope.ServiceProvider.GetService<T>();

        public static T[] ResolveAll<T>(this IServiceScope scope) =>
            scope.ServiceProvider.GetServices<T>().ToArray();

        public static IConfiguration ResolveConfig_(this IServiceScope scope) =>
            scope.Resolve_<IConfiguration>();

        public static void UseLib(this IServiceCollection collection)
        {
            collection.AddSingleton(x => HttpClientManager.Instance.DefaultClient);
            collection.AddSingleton<IDisposeComponent, LibCoreDisposeComponent>();
        }
    }
}

namespace System
{
    public static class IocExtension____
    {
        //
    }
}
