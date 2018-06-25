using Autofac;
using Autofac.Builder;
using Autofac.Extensions.DependencyInjection;
using Lib.helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lib.ioc
{
    public static class AutofacExtension
    {
        public static IServiceProvider AsServiceProvider(this IContainer context) =>
            new AutofacServiceProvider(context);

        /// <summary>
        /// 找出所有实例
        /// </summary>
        public static T[] ResolveAll<T>(this ILifetimeScope scope, string name = null)
        {
            return scope.Resolve_<IEnumerable<T>>().ToArray();
        }

        /// <summary>
        /// 创建实例
        /// </summary>
        public static T Resolve_<T>(this ILifetimeScope scope, string name = null)
        {
            if (ValidateHelper.IsPlumpString(name))
            {
                return scope.ResolveNamed<T>(name);
            }
            else
            {
                return scope.Resolve<T>();
            }
        }

        public static T ResolveKeyed_<T>(this ILifetimeScope scope, object serviceKey) =>
            scope.ResolveKeyed<T>(serviceKey);

        public static T ResolveOptionalKeyed_<T>(this ILifetimeScope scope, object serviceKey)
            where T : class =>
            scope.ResolveOptionalKeyed<T>(serviceKey);

        /// <summary>
        /// 没有注册就返回null
        /// </summary>
        public static T ResolveOptional_<T>(this ILifetimeScope scope, string name = null) where T : class
        {
            if (ValidateHelper.IsPlumpString(name))
            {
                return scope.ResolveOptionalNamed<T>(name);
            }
            else
            {
                return scope.ResolveOptional<T>();
            }
        }
    }
}
