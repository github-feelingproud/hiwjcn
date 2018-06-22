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

        public static T Resolve_<T>(this IServiceScope scope)
        {
            return scope.ServiceProvider.GetRequiredService<T>();
        }

        public static T ResolveOptional_<T>(this IServiceScope scope)
        {
            return scope.ServiceProvider.GetService<T>();
        }

        public static T[] ResolveAll<T>(this IServiceScope scope)
        {
            return scope.ServiceProvider.GetServices<T>().ToArray();
        }
    }
}
