using Autofac;
using Autofac.Builder;
using Autofac.Integration.Mvc;
using Lib.cache;
using Lib.core;
using Lib.helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using Lib.extension;
using System.Data;
using System.Data.Entity;
using Lib.mq;
using Microsoft.Extensions.DependencyInjection;

namespace Lib.ioc
{
    public static class AutofacExtension
    {
        /// <summary>
        /// 给mvc提供依赖注入功能
        /// </summary>
        public static AutofacDependencyResolver AutofacDependencyResolver_(this IContainer context) =>
            new AutofacDependencyResolver(context);
        
        //public static IServiceProvider AsServiceProvider(this IContainer context) => 
        //    new AutofacServiceProvider(context);

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

        /// <summary>
        /// 不自动dispose对象
        /// </summary>
        public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> DisableAutoDispose<TLimit, TActivatorData, TRegistrationStyle>(this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> builder) =>
            builder.ExternallyOwned();

        public const string HTTPCONTEXT_AUTOFAC_SCOPE_KEY = "ioc.autofac.scope.key";

        public static void SetAutofacScope(this HttpContext context, ILifetimeScope scope, string context_key = null)
        {
            context.Items[context_key ?? HTTPCONTEXT_AUTOFAC_SCOPE_KEY] = scope ??
                throw new ArgumentNullException(nameof(scope));
        }

        /// <summary>
        /// 获取httpcontext.item中的scope对象，需要配置httpmodule
        /// </summary>
        public static ILifetimeScope GetAutofacScope(this HttpContext context, string context_key = null)
        {
            var obj = context.Items[context_key ?? HTTPCONTEXT_AUTOFAC_SCOPE_KEY];
            if (obj is ILifetimeScope scope)
            {
                return scope;
            }
            throw new Exception("请求中没有缓存autofac scope");
        }

        /// <summary>
        /// 请求上下文的autofac scope
        /// 可以直接拿来resolve
        /// AutofacDependencyResolver.Current.RequestLifetimeScope
        /// </summary>
        public static ILifetimeScope AutofacRequestLifetimeScope(this HttpContext context) =>
            AutofacDependencyResolver.Current.RequestLifetimeScope;
    }
}
