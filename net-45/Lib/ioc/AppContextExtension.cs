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
    public static class IocContextExtension
    {
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
